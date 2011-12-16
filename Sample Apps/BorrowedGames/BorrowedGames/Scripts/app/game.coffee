library = Backbone.Model.extend
  reviewUrl: -> "http://www.google.com/search?q=" + encodeURIComponent(@name() + " ") + "site:gamespot.com&btnI=3564"

  name: -> @get("Name")

  console: -> @get("Console")

  shortName: ->
    name = @name()
    
    name = name.substring(0, 40) + "... " if name > 45

    name += " (" + @console() + ")"

  notInterested: ->
    $.post(@get("NotInterested"), { }, =>
      @deleted = true
      @change()
    )

  wantGame: ->
    $.post(@get("WantGame"), { }, =>
      @wanted = true
      @change()
    )

  deleted: false

  wanted: false

libraries = Backbone.Collection.extend
  model: library
  url: "/games/preferred"

this.preferredGamesView = Backbone.View.extend
  initialize: ->
    _.bindAll this, 'render'

    @preferredGames = new libraries()

    @preferredGames.bind 'reset', @render

    @preferredGames.fetch()

  render: ->
    $(@el).empty()

    if(@preferredGames.length == 0)
      $(@el).html(
        '
        <div class="info" id="showFriends" style="padding-left: 30px">
          Games you don\'t own (that your friends have) will show up here.
        </div>
        '
      )
    else
      @preferredGames.each (library) =>

        view = new preferredGameView
          model: library
        
        view.initialize()

        $(@el).append view.render().el

      $(@el).append($("<div />").css({ clear: "both" }))

preferredGameView = Backbone.View.extend
  className: 'gameBox'

  initialize: ->
    _.bindAll this, "render", "apply"

    @model.bind 'change', @apply

  apply: ->
    $(@el).fadeOut() if(@model.deleted || @model.wanted)

  events:
    "click .cancel": "notInterested"
    "click .request": "wantGame"

  notInterested: -> @model.notInterested()

  wantGame: -> @model.wantGame()

  render: ->
    game = $.tmpl(@gameTemplate, { gameName: @model.shortName(), searchString: @model.reviewUrl() })

    $(@el).html(game)

    requestLink = game.find(".request")

    toolTip.init(
      requestLink,
      "WantGame",
      "Click here to request the game.",
      "You get the idea...<br/>Request game.",
      -> game.offset().left + 100
      -> requestLink.offset().top
    )

    toolTip.init(
      game.find(".cancel"),
      "NotInterested",
      "Not interested?<br/>Click to remove it.",
      "You get the idea...<br/>Remove game.",
      -> game.offset().left + 100,
      -> game.offset().top + -25
    )

    return this

  gameTemplate:
    '
    <div style="padding-bottom: 5px; margin-bottom: 10px; border-bottom: 1px silver solid; height: 20px">
      <a href="javascript:;" id="closeLink${gameId}_${userId}" 
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
