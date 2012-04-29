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

  undoRequest: ->
    $.post(@get("DeleteWant"), { }, =>
      preferred.getPreferredGames()
      @change()
    )

  returnGame: ->
    $.post(@get("ReturnGame"), { }, =>
      preferred.getPreferredGames()
      @change()
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
  className: 'border'

  initialize: ->
    @model.bind 'change', @apply, @

  apply: ->
    $(@el).fadeOut()

  events:
    "click .cancel": "delete"

  delete: ->
    @model.undoRequest() if !@model.canReturnGame()

    @model.returnGame() if @model.canReturnGame()

  render: ->
    @renderRequestedGame() if !@model.canReturnGame()

    @renderBorrowedGame() if @model.canReturnGame()

    return this

  renderRequestedGame: ->
    game = $.tmpl(@requestedGameTemplate, { gameName: @model.shortName(), owner: @model.owner() })

    $(@el).html(game)

    toolTip.init(
      game.find(".cancel"),
      "UndoRequest",
      "Don't want the game?<br/>Click to undo request.",
      "You get the idea...<br/>Undo request.",
      -> game.find(".cancel").offset().left - 125,
      -> game.find(".cancel").offset().top - 75
    )

  renderBorrowedGame: ->
    game = $.tmpl(@borrowedGameTemplate, { gameName: @model.shortName(), owner: @model.owner() })

    $(@el).html(game)

    toolTip.init(
      game.find(".cancel"),
      "ReturnGame",
      "All done with the game?<br/>Click to mark it as returned.",
      "You get the idea...<br/>Return game.",
      -> game.find(".cancel").offset().left - 125,
      -> game.find(".cancel").offset().top - 75
    )

  borrowedGameTemplate:
    '
  <div class="menubar">
    <a href="javascript:;"
       style="text-decoration: none; color: black; float: right; padding-left: 15px"
       class="cancel">&nbsp;</a>
    <div style="clear: both">&nbsp;</div>
  </div>
    <span style="float: right; font-size: 30px; color: silver; margin-right: 10px" class="brand">
      Borrowed
    </span>
    <div style="font-size: 20px">${gameName}</div>
    <div>${owner}</div>
    '

  requestedGameTemplate:
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
