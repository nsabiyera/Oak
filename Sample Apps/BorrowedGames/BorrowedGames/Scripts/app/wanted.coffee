wantedGamesUrl = ""

this.wanted =
  init: (urls) ->
    wantedGamesUrl = urls.wantedGamesUrl
    @view = new wantedGamesView()

  getWantedGames: -> @view.refresh()

wantedGame = Backbone.Model.extend
  name: -> @get("name")

  console: -> @get("console")

  owner: -> @get("owner").handle

  canReturnGame: -> @get("returnGame")

  daysLeft: -> @get("daysLeft")

  shortName: ->
    name = @name()

    name = name.substring(0, 40) + "... " if name.length > 41

    name += " (" + @console() + ")"

  undoRequest: (callback) ->
    $.post(@get("deleteWant"), { }, =>
      preferred.getPreferredGames()
      @change()
      callback()
    )

  returnGame: (callback) ->
    $.post(@get("returnGame"), { }, =>
      preferred.getPreferredGames()
      @change()
      callback()
    )

wantedGames = Backbone.Collection.extend
  model: wantedGame
  url: -> wantedGamesUrl

wantedGamesView = Backbone.View.extend
  el: "#wantedGames"

  initialize: ->
    @wantedGames = new wantedGames()

    @wantedGames.bind 'reset', @render, @

    @wantedGames.fetch()

  refresh: ->
    @wantedGames.fetch()

  render: ->
    $(@el).empty()

    @wantedGames.each (game) => @addGame(game)

  addGame: (game) ->
    view = new wantedGameView
      model: game

    view.render()

    $(@el).append view.el

wantedGameView = Backbone.View.extend
  tagName: 'tr'

  className: ''

  initialize: ->
    @model.bind 'change', @apply, @

  apply: ->
    $(@el).fadeOut()

  events:
    "click .cancel": "delete"

  delete: ->
    el = @el

    @model.undoRequest(-> $(el).fadeOut()) if !@model.canReturnGame()

    @model.returnGame(-> $(el).fadeOut()) if @model.canReturnGame()

  render: ->
    @renderRequestedGame() if !@model.canReturnGame()

    @renderBorrowedGame() if @model.canReturnGame()

    return this

  renderRequestedGame: ->
    game = $.tmpl(@requestedGameTemplate, { gameName: @model.shortName(), owner: @model.owner() })

    game.find(".cancel").tooltip({ title: "<span style='font-size: 16px'>cancel game request</span>" })

    $(@el).html(game)

  renderBorrowedGame: ->
    daysLeft = @model.daysLeft()
    daysLeftClass = "label-info"
    daysLeftText = daysLeft + " day(s) left"
    daysLeftClass = "label-warning" if daysLeft <= 10
    daysLeftClass = "label-important" if daysLeft <= 0
    daysLeftText = "overdue, return game" if daysLeft <= 0
    

    game = $.tmpl(@borrowedGameTemplate, { gameName: @model.shortName(), owner: @model.owner(), daysLeft: daysLeftText, daysLeftClass: daysLeftClass })

    game.find(".cancel").tooltip({ title: "<span style='font-size: 16px'>the game has been returned</span>" })

    $(@el).html(game)

  borrowedGameTemplate:
    '
    <td class="span1">
      <span class="label label-success">currently borrowing</span>
      <span class="label ${daysLeftClass}">${daysLeft}</span>
    </td>
    <td>${gameName}</td>
    <td>${owner}</td>
    <td class="span2"><i class="icon-remove cancel" style="cursor: pointer"></i></td>
    '

  requestedGameTemplate:
    '
    <td class="span1">
      <span class="label label-inverse">requested</span>
    </td>
    <td>${gameName}</td>
    <td>${owner}</td>
    <td class="span2"><i class="icon-remove cancel" style="cursor: pointer">&nbsp;</i></td>
    '
