preferredGamesUrl = ""

this.preferred =
  init: (urls) ->
    preferredGamesUrl = urls.preferredGamesUrl

    @view = new preferredGamesView()
    
  getPreferredGames: -> @view.refresh()

library = Backbone.Model.extend
  reviewUrl: -> "http://www.google.com/search?q=" + encodeURIComponent(@name() + " ") + "site:gamespot.com&btnI=3564"

  name: -> @get("Name")

  console: -> @get("Console")

  shortName: ->
    name = @name()
    
    name = name.substring(0, 40) + "... " if name.length > 41

    name += " (" + @console() + ")"

  notInterested: (callback) ->
    $.post(@get("NotInterested"), { }, =>
      @deleted = true
      unwanted.getUnwantedGames()
      @change()
      callback()
    )

  isFavorited: -> @get("IsFavorited")

  favorite: ->
    $.post(@get("FavoriteGame"), { }, =>
      preferred.getPreferredGames()
    )

  unfavorite: ->
    $.post(@get("UnfavoriteGame"), { }, =>
      preferred.getPreferredGames()
    )

  wantGame: (callback) ->
    $.post(@get("WantGame"), { }, =>
      @wanted = true
      wanted.getWantedGames()
      @change()
      callback()
    )

  owner: ->
    @get("Owner").Handle

  deleted: false

  wanted: false

libraries = Backbone.Collection.extend
  model: library
  url: -> preferredGamesUrl

preferredGamesView = Backbone.View.extend
  el: "#preferredGames"

  initialize: ->
    @preferredGames = new libraries()

    @preferredGames.bind 'reset', @render, @

    @preferredGames.fetch()

  refresh: ->
    @preferredGames.fetch()

  render: ->
    $(@el).empty()

    @preferredGames.each (library) =>
      view = new preferredGameView
        model: library

      $(@el).append view.render().el

    if(@preferredGames.length == 0)
      $(@el).html(
        '
        <div class="info" id="showFriends" style="padding-left: 30px">
          Games you don\'t own (that your friends have) will show up here.
        </div>
        '
      )

preferredGameView = Backbone.View.extend
  tagName: "tr"

  className: ''

  initialize: -> @model.bind 'change', @apply, @

  apply: ->
    $(@el).fadeOut() if(@model.deleted || @model.wanted)

  events:
    "click .cancel": "notInterested"
    "click .request": "wantGame"
    "click .favorite": "toggleFavorite"

  notInterested: ->
    el = @el
    @model.notInterested(-> $(el).fadeOut())

  wantGame: ->
    el = @el
    @model.wantGame(-> $(el).fadeOut())

  toggleFavorite: ->
    if @model.isFavorited()
      $(@el).find(".icon-star").tooltip('hide')
      $(@el).find(".icon-star-empty").tooltip('hide')
      @model.unfavorite()
    else
      $(@el).find(".icon-star").tooltip('hide')
      $(@el).find(".icon-star-empty").tooltip('hide')
      @model.favorite()

  render: ->
    starClass = "icon-star-empty"

    starClass = "icon-star" if @model.isFavorited()

    game = $.tmpl(@gameTemplate, { gameName: @model.shortName(), searchString: @model.reviewUrl(), owner: @model.owner(), starClass: starClass })

    $(@el).html(game)

    game.find(".cancel").tooltip({ "title": "<span style='font-size: 16px'>if you aren't interested in the game, put it into qurantine<span>", "placement": "top" })

    game.find(".request").tooltip({ "title": "<span style='font-size: 16px'>request the game from " + @model.owner() + "<span>", "placement": "top" })

    game.find(".icon-star-empty").tooltip({ "title": "<span style='font-size: 16px'>bring the game to the top of your list<span>", "placement": "top" })

    game.find(".icon-star").tooltip({ "title": "<span style='font-size: 16px'>bring the game down from its pedestal<span>", "placement": "top" })

    return this

  gameTemplate:
    '
      <td><a href="${searchString}" target="_blank">${gameName}</a></td>
      <td>${owner}</td>
      <td class="span2">
        <i href="javascript:;" class="favorite ${starClass}" style="cursor: pointer; color: red"></i>
        <i href="javascript:;" class="request icon-arrow-up" style="cursor: pointer"></i>
        <i href="javascript:;" class="cancel icon-arrow-down" style="cursor: pointer"></i>
      </td>
    '
