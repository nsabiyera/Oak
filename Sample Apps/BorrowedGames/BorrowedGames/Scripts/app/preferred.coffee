preferredGames = null

initView = ->
  preferredGames = $("#preferredGames")

addGameToPage = (game) ->
  $game = gameElementFor(game)

  preferredGames.append $game

setRequested = ($game) ->
 
    
gameElementFor = (game) ->
  searchString =
    "http://www.google.com/search?q=" +
    encodeURIComponent(game.Name + " ") +
    "site:gamespot.com&btnI=3564"
  
  userId = game.Owner.Id

  gameName = game.Name

  gameName = game.Name.substring(0, 40) + "... " if game.Name.length > 45

  gameName += " (" + game.Console + ")"

  $game = $.tmpl gameTemplate, { gameId: game.Id, gameName, searchString, owner: game.Owner.Handle, userId }
  
  $game.game = game

  $game.takeActionLink = -> $game.find("#takeAction#{game.Id}_#{userId}")

  $game.closeLink = -> $game.find("#closeLink#{game.Id}_#{userId}")

  wireUpGameEventHandlers $game

  return $game

wireUpGameEventHandlers = ($game) ->
  game = $game.game
  takeAction = $game.takeActionLink()
  closeLink = $game.closeLink()
  userId = $game.game.Owner.Id

  takeAction.click(->
    $.post(game.WantGame,
    { },
    -> $game.fadeOut(-> wanted.getWantedGames())
    )
  )

  toolTip.init(
    takeAction,
    "WantGame",
    "Click here to request the game.",
    "You get the idea...<br/>Request game.",
    -> $game.offset().left + 100
    -> takeAction.offset().top
  )

  toolTip.init(
    closeLink,
    "NotInterested",
    "Not interested?<br/>Click to remove it.",
    "You get the idea...<br/>Remove game.",
    -> $game.offset().left + 100,
    -> $game.offset().top + -25
  )

  closeLink.click(->
    $.post(game.NotInterested,
    { },
    -> $game.fadeOut())
  )


this.preferred =
  init: (urls) ->
    initView()

    @urls = urls

    @getPreferredGames()

  getPreferredGames: ->
    $.getJSON(
      @urls.preferredGamesUrl,
      (games) ->
        preferredGames.html ''

        addGameToPage(game) for game in games

        preferredGames.append $("<div />").css(clear: "both")

        preferredGames.html(
          '
          <div class="info" id="showFriends" style="padding-left: 30px">
            Games you don\'t own (that your friends have) will show up here.
          </div>
          '
        ) if(!games.length)
      )

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
