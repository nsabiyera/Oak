wantedGamesUrl = ""

this.wanted =
  init: (urls, div) ->
    wantedGamesUrl = urls.wantedGamesUrl
    @view = new wantedGamesView()
    @view.initialize()
    div.html(@view.el)

  getWantedGames: -> @view.refresh()

wantedGame = Backbone.Model.extend()

wantedGames = Backbone.Collection.extend
  model: wantedGame
  url: -> wantedGamesUrl

wantedGamesView = Backbone.View.extend
  initialize: ->
    _.bindAll this, 'render'

    @wantedGames = new wantedGames()

    @wantedGames.bind 'reset', @render

    @wantedGames.fetch()

  render: ->
    $(@el).empty()

    if(@wantedGames.length != 0)
      @wantedGames.each (game) =>
        view = new wantedGameView
          model: game

        view.initialize()
        view.render()

        $(@el).append view.el

wantedGameView = Backbone.View.extend
  initialize: ->
    _.bindAll this, "render"

  render: ->

  gameTemplate:
    '
    <div class="border">
      <span id="status${gameId}_${userId}" style="float: right; font-size: 30px; color: silver; margin-right: 10px" class="brand">
        Requested
      </span>
      <div style="font-size: 20px">${gameName}</div>
      <div>${owner}</div>
    </div>
    '


initView = ->
  wantedGames = $("#wantedGames")

addGameToPage = (game) ->
  gameName = game.Name

  gameName = game.Name.substring(0, 40) + "... " if game.Name.length > 45

  gameName += " (" + game.Console + ")"

  $game = $.tmpl gameTemplate, { gameName, owner: game.Owner.Handle }

  wantedGames.append $game


gameTemplate =

