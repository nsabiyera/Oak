wantedGamesUrl = ""

this.wanted =
  init: (urls) ->
    wantedGamesUrl = urls.wantedGamesUrl
    @view = new wantedGamesView()

  getWantedGames: -> @view.refresh()

wantedGame = Backbone.Model.extend
  name: -> @get("Name")

  console: -> @get("Console")

  owner: -> @get("Owner").Handle

  canReturnGame: -> @get("ReturnGame")

  shortName: ->
    name = @name()
    
    name = name.substring(0, 40) + "... " if name.length > 41

    name += " (" + @console() + ")"

  undoRequest: (callback) ->
    $.post(@get("DeleteWant"), { }, =>
      preferred.getPreferredGames()
      @change()
      callback()
    )

  returnGame: (callback) ->
    $.post(@get("ReturnGame"), { }, =>
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

    if(@wantedGames.length > 0)
      $("#wantedGamesHeader").show()
    else
      $("#wantedGamesHeader").hide()

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

    $(@el).html(game)

  renderBorrowedGame: ->
    game = $.tmpl(@borrowedGameTemplate, { gameName: @model.shortName(), owner: @model.owner() })

    $(@el).html(game)

  borrowedGameTemplate:
    '
    <td class="span2">
      <div class="btn-group">
        <a class="btn dropdown-toggle span2" data-toggle="dropdown" href="javascript:;">borrowed <span class="caret"></span></a>
        <ul class="dropdown-menu">
          <li><a href="javascript:;" class="cancel">return game</a></li>
        </ul>
      </div>
    </td>
    <td>${gameName}</td>
    <td>${owner}</td>
    '

  requestedGameTemplate:
    '
    <td class="span2">
      <div class="btn-group">
        <a class="btn dropdown-toggle span2" data-toggle="dropdown" href="javascript:;">requested <span class="caret"></span></a>
        <ul class="dropdown-menu">
          <li><a href="javascript:;" class="cancel">cancel request</a></li>
        </ul>
      </div>
    </td>
    <td>${gameName}</td>
    <td>${owner}</td>
    '
