unwantedGamesUrl = ""

this.unwanted =
  init: (urls) ->
    unwantedGamesUrl = urls.unwantedGamesUrl

    @view = new unwantedGamesView()

  getUnwantedGames: -> @view.refresh()

unwantedGame = Backbone.Model.extend
  name: -> @get("Name")

  shortName: ->
    name = @name()
    
    name = name.substring(0, 20) + "... " if name.length > 21

    name += " (" + @console() + ")"

  console: -> @get("Console")

  undo: (callback) ->
    $.post(@get("UndoNotInterested"), { }, =>
      preferred.getPreferredGames()
      @change()
      callback()
    )

unwantedGames = Backbone.Collection.extend
  model: unwantedGame
  url: -> unwantedGamesUrl

unwantedGamesView = Backbone.View.extend
  el: "#unwantedGames"

  initialize: ->
    @unwantedGames = new unwantedGames()

    @unwantedGames.bind 'reset', @render, @

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
  tagName: "tr"

  initialize: ->
    @model.bind 'change', @apply, @

  apply: ->
    $(@el).fadeOut()

  events:
    "click .cancel": "undo"

  undo: ->
    el = @el
    @model.undo(-> $(el).fadeOut())

  render: ->
    game = $.tmpl(@gameTemplate, { gameName: @model.shortName() })

    $(@el).html(game)

    game.find(".cancel").tooltip({ title: "<span style='font-size: 16px'>remove the game from qurantine</span>" })

    return this

  gameTemplate:
    '
    <td class="span1">
     <span class="label label-important">qurantined</span>
    </td>
    <td>${gameName}</td>
    <td class="span1"><i class="icon-arrow-up cancel" style="cursor: pointer"></i></td>
    '
