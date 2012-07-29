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

  notInterested: ->
    el = @el
    @model.notInterested(-> $(el).fadeOut())

  wantGame: ->
    el = @el
    @model.wantGame(-> $(el).fadeOut())

  render: ->
    game = $.tmpl(@gameTemplate, { gameName: @model.shortName(), searchString: @model.reviewUrl(), owner: @model.owner() })

    $(@el).html(game)

    game.find(".cancel").tooltip({ "title": "<span style='font-size: 16px'>if you aren't interested in the game, put it into qurantine<span>", "placement": "left" })

    game.find(".request").tooltip({ "title": "<span style='font-size: 16px'>request the game from " + @model.owner() + "<span>", "placement": "left" })

    return this

  gameTemplate:
    '
      <td><a href="${searchString}" target="_blank">${gameName}</a></td>
      <td>${owner}</td>
      <td class="span1">
        <div class="btn-group">
          <a class="btn dropdown-toggle span2 btn-primary" data-toggle="dropdown" href="javascript:;">options <span class="caret"></span></a>
          <ul class="dropdown-menu">
            <li><a href="javascript:;" class="request">request game</a></li>
            <li><a href="javascript:;" class="cancel">not interested</a></li>
          </ul>
        </div>
      </td>
    '
