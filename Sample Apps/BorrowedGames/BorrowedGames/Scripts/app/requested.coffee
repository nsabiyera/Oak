requestedGames = null

initView = ->
  requestedGames = $("#requestedGames")

addGameToPage = (game) ->
  gameName = game.Name

  gameName = game.Name.substring(0, 40) + "... " if game.Name.length > 45

  gameName += " (" + game.Console + ")"

  $game = $.tmpl gameTemplate, { gameName, owner: game.Owner.Handle }

  requestedGames.append $game

this.requested =
  init: (urls) ->
    initView()

    @urls = urls

    @getRequestedGames()

  getRequestedGames: ->
    $.getJSON(
      @urls.requestedGamesUrl,
      (games) ->
        requestedGames.html ''
        requestedGames.hide()
        addGameToPage(game) for game in games
        requestedGames.fadeIn()
    )

gameTemplate =
  '
  <div class="border">
    <span id="status${gameId}_${userId}" style="float: right; font-size: 30px; color: silver; margin-right: 10px" class="brand">
      Requested
    </span>
    <div style="font-size: 20px">${gameName}</div>
    <div>${owner}</div>
  </div>
  '
