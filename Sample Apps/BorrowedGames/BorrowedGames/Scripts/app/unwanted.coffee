unwantedGamesUrl = ""

this.unwanted =
  init: (urls, div) ->
    unwantedGamesUrl = urls.unwantedGamesUrl

    @view = new unwantedGamesView()

    @view.initialize()

    div.html(@view.el)

  getUnWantedGames: -> @view.refresh()

unwantedGame = Backbone.Model.extend
  name: -> @get("Name")

  shortName: ->
    name = @name()
    
    name = name.substring(0, 40) + "... " if name > 45

    name += " (" + @console() + ")"

  undo: ->
    alert(@get("UndoNotInterested"))

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

    @unwantedGames.each (game) =>
      view = new unwantedGameView
        model: game

      view.initialize()

      $(@el).append view.render().el

unwantedGameView: Backbone.View.extend
  initialize: ->
    _.bindAll this, "render"

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
      "Want to give the game another shot? Remove it from quranteen.",
      "You get the idea...<br/>Remove it from quranteen.",
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
      <a style="color: black;" href="${searchString}" target="_blank">${gameName}</a><br/>
    </div>
    <div style="font-size: 12px; height: 30px; padding-bottom: 3px">
      ${owner}
    </div>
    <div style="padding-bottom: 5px; margin-bottom: 10px; border-top: 1px silver solid">
      <a href="javascript:;" class="request" style="font-size: 12px">request game</a>
    </div>
    '
