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

  owner: -> @get("Owner").Handle

  shortName: ->
    name = @name()
    
    name = name.substring(0, 40) + "... " if name > 45

    name += " (" + @console() + ")"

  undoRequest: ->
    $.post(@get("DeleteWant"), { }, =>
      preferred.getPreferredGames()
      @change()
    )

wantedGames = Backbone.Collection.extend
  model: wantedGame

  url: -> wantedGamesUrl

wantedGamesView = Backbone.View.extend
  tagName: "span"

  initialize: ->
    _.bindAll this, 'render'

    @wantedGames = new wantedGames()

    @wantedGames.bind 'reset', @render

    @wantedGames.fetch()

  refresh: ->
    @wantedGames.fetch()

  addGame: (game) ->
    view = new wantedGameView
      model: game

    view.initialize()

    view.render()

    $(@el).append view.el

  render: ->
    $(@el).empty()

    @wantedGames.each (game) => @addGame(game)

wantedGameView = Backbone.View.extend
  className: 'border'

  initialize: ->
    _.bindAll this, "render", "apply"

    @model.bind 'change', @apply

  apply: ->
    $(@el).fadeOut()

  events:
    "click .cancel": "undoRequest"

  undoRequest: -> @model.undoRequest()

  render: ->
    game = $.tmpl(@gameTemplate, { gameName: @model.shortName(), owner: @model.owner() })

    $(@el).html(game)

    toolTip.init(
      game.find(".cancel"),
      "UndoRequest",
      "Don't want the game?<br/>Click to undo request.",
      "You get the idea...<br/>Undo request.",
      -> game.find(".cancel").offset().left - 125,
      -> game.find(".cancel").offset().top - 75
    )

    return this

  gameTemplate:
    '
  <div class="menubar">
    <a href="javascript:;"
       style="text-decoration: none; color: black; float: right; padding-left: 15px"
       class="cancel">&nbsp;</a>
    <div style="clear: both">&nbsp;</div>
  </div>
    <span style="float: right; font-size: 30px; color: silver; margin-right: 10px" class="brand">
      Requested
    </span>
    <div style="font-size: 20px">${gameName}</div>
    <div>${owner}</div>
    '
