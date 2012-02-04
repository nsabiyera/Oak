unwantedGamesUrl = ""

this.unwanted =
  init: (urls, div) ->
    unwantedGamesUrl = urls.unwantedGamesUrl

    @view = new unwantedGamesView()

    div.html(@view.el)

  getUnwantedGames: -> @view.refresh()

unwantedGame = Backbone.Model.extend
  name: -> @get("Name")

  shortName: ->
    name = @name()
    
    name = name.substring(0, 20) + "... " if name.length > 21

    name += " (" + @console() + ")"

  console: -> @get("Console")

  undo: ->
    $.post(@get("UndoNotInterested"), { }, =>
      preferred.getPreferredGames()
      @change()
    )

unwantedGames = Backbone.Collection.extend
  model: unwantedGame
  url: -> unwantedGamesUrl

unwantedGamesView = Backbone.View.extend
  initialize: ->
    _.bindAll this, 'render'

    @unwantedGames = new unwantedGames()

    @unwantedGames.bind 'reset', @render

    @unwantedGames.fetch()

  refresh: ->
    @unwantedGames.fetch()

  render: ->
    $(@el).empty()

    @unwantedGames.each (game) => @addGame(game)
    
    $(@el).append($("<div />").css({ clear: "both" }))

  addGame: (game) ->
    view = new unwantedGameView
      model: game

    view.render()

    $(@el).append view.el

unwantedGameView = Backbone.View.extend
  className: 'gameBoxSmall'

  initialize: ->
    _.bindAll this, "render", "apply"

    @model.bind 'change', @apply

  apply: ->
    $(@el).fadeOut()

  events:
    "click .cancel": "undo"

  undo: -> @model.undo()

  render: ->
    game = $.tmpl(@gameTemplate, { gameName: @model.shortName() })

    $(@el).html(game)

    toolTip.init(
      game.find(".cancel"),
      "UndoUnwantedGame",
      "Want to give the game another shot?<br/>Remove it from qurantine.",
      "You get the idea...<br/>Remove it from qurantine.",
      -> game.offset().left + 100,
      -> game.offset().top + -25
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
    <div style="font-size: 12px; height: 70px; padding-bottom: 3px">
      ${gameName}<br/>
    </div>
    '
