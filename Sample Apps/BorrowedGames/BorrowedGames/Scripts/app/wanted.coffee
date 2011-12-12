wantedGames = null

initView = ->
  wantedGames = $("#wantedGames")

addGameToPage = (game) ->
  gameName = game.Name

  gameName = game.Name.substring(0, 40) + "... " if game.Name.length > 45

  gameName += " (" + game.Console + ")"

  $game = $.tmpl gameTemplate, { gameName, owner: game.Owner.Handle }

  wantedGames.append $game

this.wanted =
  init: (urls) ->
    initView()

    @urls = urls

    @getWantedGames()

  getWantedGames: ->
    $.getJSON(
      @urls.wantedGamesUrl,
      (games) ->
        wantedGames.html ''
        wantedGames.hide()
        addGameToPage(game) for game in games
        wantedGames.fadeIn()
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
