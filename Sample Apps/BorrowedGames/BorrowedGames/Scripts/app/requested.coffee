requestedGamesUrl = ""

this.requested =
  init: (urls) ->
    requestedGamesUrl = urls.requestedGamesUrl

    @view = new requestedGamesView()

  getRequestedGames: -> @view.refresh()

requestedGame = Backbone.Model.extend
  name: -> @get("Name")

  console: -> @get("Console")

  requestedBy: -> @get("RequestedBy").Handle

  shortName: ->
    name = @name()
    
    name = name.substring(0, 40) + "... " if name.length > 41

    name += " (" + @console() + ")"

  giveGame: (callback) ->
    $.post(@get("GiveGame"), { }, =>
      requested.getRequestedGames()
      callback()
    )

  gameReturned: (callback) ->
    $.post(@get("GameReturned"), { }, =>
      requested.getRequestedGames()
      callback()
    )

  canGiveGame: ->
    !!@get("GiveGame")

requestedGames = Backbone.Collection.extend
  model: requestedGame
  url: -> requestedGamesUrl

requestedGamesView = Backbone.View.extend
  el: "#requestedGames"

  initialize: ->
    @requestedGames = new requestedGames()

    @requestedGames.bind 'reset', @render, @

    @requestedGames.fetch()

  refresh: ->
    @requestedGames.fetch()

  render: ->
    $(@el).empty()

    if @requestedGames.length == 0
      $("#requestedGamesHeader").hide()
    else
      $("#requestedGamesHeader").show()

    @requestedGames.each (game) => @addGame(game)

  addGame: (game) ->
    view = new requestedGameView
      model: game

    view.render()

    $(@el).append view.el

requestedGameView = Backbone.View.extend
  tagName: "tr"

  events:
    "click .check" : "giveGame"
    "click .cancel" : "gameReturned"

  giveGame: ->
    el = @el
    @model.giveGame(-> $(el).find(".check").tooltip("hide"))

  gameReturned: ->
    el = @el
    @model.gameReturned(-> $(el).find(".cancel").tooltip("hide"))

  render: ->
    game = @genCanGiveTemplate() if @model.canGiveGame()

    game = @genReturnGame() if !@model.canGiveGame()

    $(@el).html(game)

    return this

  genCanGiveTemplate: ->
    gen = $.tmpl(@canGiveGameTemplate, { requestedBy: @model.requestedBy(), gameName: @model.shortName() })
    requestedBy = @model.requestedBy()
    gen.find(".check").tooltip({ title: "<span style='font-size: 16px'>click this when you have given " + requestedBy + " the game, it'll start the count down for when the game needs to be returned</span>"})
    return gen

  genReturnGame: ->
    gen = $.tmpl(@returnGameTemplate, { requestedBy: @model.requestedBy(), gameName: @model.shortName() })
    gen.find(".cancel").tooltip({ title: "<span style='font-size: 16px'>the game has been returned to me</span>"})
    return gen

  returnGameTemplate:
    '
    <td>${requestedBy} is currently <span class="label label-success">borrowing</span> ${gameName}</td>
    <td>
        <i class="cancel icon-ok" style="cursor: pointer" href="javascript:;"></i>
    </td>
    '

  canGiveGameTemplate:
    '
    <td>${requestedBy} is <span class="label label-inverse">requesting</span> ${gameName}</td>
    <td>
        <i class="check icon-share-alt" style="cursor: pointer" href="javascript:;"></i>
    </td>
    '
