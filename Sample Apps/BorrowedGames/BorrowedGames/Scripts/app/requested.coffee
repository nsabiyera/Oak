requestedGamesUrl = ""

this.requested =
  init: (urls) ->
    requestedGamesUrl = urls.requestedGamesUrl

    @view = new requestedGamesView()

requestedGame = Backbone.Model.extend
  name: -> @get("Name")

  console: -> @get("Console")

  requestedBy: -> @get("RequestedBy").Handle

  shortName: ->
    name = @name()
    
    name = name.substring(0, 40) + "... " if name.length > 41

    name += " (" + @console() + ")"

  givenGame: ->
    alert("I have given him the game")





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

  render: ->
    game = $.tmpl(@gameTemplate, { })

    $(@el).html(game)

    return this

  gameTemplate:
    '
    <div class="border">
      <div style="float: right; margin-top: 15px; margin-right: 20px; font-size: 20px">
          <a class="check" href="javascript:alert("todo");">I have given him the game</a>
      </div>
    <div style="width: 60%; font-size: 20px">@@amirrajan is requesting<br /> Star Wars: The Force Unleashed (XBOX360)</div>
    '
