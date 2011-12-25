wantedGamesUrl = ""

this.wanted =
  init: (urls, div) ->
    wantedGamesUrl = urls.wantedGamesUrl
    @view = new wantedGamesView()
    @view.initialize()
    div.html(@view.el)

  getWantedGames: -> @view.refresh()

wantedGame = Backbone.Model.extend
  name: -> @get("Name")

  console: -> @get("Console")

  shortName: ->
    name = @name()
    
    name = name.substring(0, 40) + "... " if name > 45

    name += " (" + @console() + ")"

wantedGames = Backbone.Collection.extend
  model: wantedGame
  url: -> wantedGamesUrl

wantedGamesView = Backbone.View.extend
  initialize: ->
    _.bindAll this, 'render'

    @wantedGames = new wantedGames()

    @wantedGames.bind 'reset', @render

    @wantedGames.fetch()

  refresh: ->
    @wantedGames.refresh()

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
    game = $.tmpl(@gameTemplate, { gameName: @model.shortName() })

    $(@el).html(game)

    return this

  gameTemplate:
    '
    <div class="border">
      <span style="float: right; font-size: 30px; color: silver; margin-right: 10px" class="brand">
        Requested
      </span>
      <div style="font-size: 20px">${gameName}</div>
      <div>${owner}</div>
    </div>
    '
