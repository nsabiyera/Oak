window.BG ||= { }

window.BG.Models ||= { }

window.BG.Views ||= { }

window.BG.Collections ||= { }

class BG.Models.Library extends Backbone.Model

class BG.Collections.Libraries extends Backbone.Collection
  model: BG.Models.Library
  url: "/games/preferred"

class BG.Views.PreferredGamesView extends Backbone.View
  initialize: ->
    _.bindAll this, 'render'

    @preferredGames = new BG.Collections.Libraries

    @preferredGames.bind 'reset', @render

    @preferredGames.fetch()

  render: ->
    if(@preferredGames.length == 0)
      $(@el).html(
        '
        <div class="info" id="showFriends" style="padding-left: 30px">
          Games you don\'t own (that your friends have) will show up here.
        </div>
        '
      )

    else
      $(@el).empty()
      @preferredGames.each (library) =>
        view = new BG.Views.PreferredGameView
          model: library
        $(@el).append view.render().el

    return this

class BG.Views.PreferredGameView extends Backbone.View
  initialize: ->
    _.bindAll this, 'render'

  render: ->
    game = $.tmpl(gameTemplate, { })
    $(@el).html(game)

    $(@el).append($("<div />").css({ clear: "both" }))

    return this

gameTemplate =
  '
  <div id="game${gameId}_${userId}" class="border dropshadow" style="float: left; width: 100px; height: 160px">
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
      <a href="javascript:;" id="takeAction${gameId}_${userId}" style="font-size: 12px">request game</a>
    </div>
  </div>
  '
