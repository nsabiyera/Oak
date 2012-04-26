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

  giveGame: ->
    $.post(@get("GiveGame"), { }, =>
      requested.getRequestedGames()
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

    @requestedGames.each (game) => @addGame(game)

  addGame: (game) ->
    view = new requestedGameView
      model: game

    view.render()

    $(@el).append view.el

requestedGameView = Backbone.View.extend
  className: "border"

  events:
    "click .check" : "giveGame"

  giveGame: -> @model.giveGame()

  render: ->
    game = @genCanGiveTemplate() if @model.canGiveGame()

    game = @genReturnGame() if !@model.canGiveGame()

    $(@el).html(game)

    return this

  genCanGiveTemplate: ->
    return $.tmpl(@canGiveGameTemplate, { requestedBy: @model.requestedBy(), gameName: @model.shortName() })

  genReturnGame: ->
    return $.tmpl(@returnGameTemplate, { requestedBy: @model.requestedBy(), gameName: @model.shortName() })

  returnGameTemplate:
    '
    <div style="float: right; margin-top: 15px; margin-right: 20px; font-size: 20px">
        <a class="cancel" href="javascript:;">The game has been returned</a>
    </div>
    <div style="width: 60%; font-size: 20px">${requestedBy} is requesting<br /> ${gameName}</div>
    '

  canGiveGameTemplate:
    '
    <div style="float: right; margin-top: 15px; margin-right: 20px; font-size: 20px">
        <a class="check" href="javascript:;">I have given him the game</a>
    </div>
    <div style="width: 60%; font-size: 20px">${requestedBy} is requesting<br /> ${gameName}</div>
    '
