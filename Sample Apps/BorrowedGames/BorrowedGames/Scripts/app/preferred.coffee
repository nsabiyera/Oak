$findGame = (gameId, userId) -> $("#game#{gameId}_#{userId}")

$preferredGames = null

initView = ->
	$preferredGames = $("#preferredGames")

$addGameToPage = (game) -> 
	$game = $gameElementFor(game)

	$preferredGames.append $game

	$wireUpNotInterested $game

	$wireUpActionLink $game

$wireUpActionLink = ($game) ->
	game = $game.game

	userId = game.Owner.Id

	$takeAction = $game.find("#takeAction#{game.Id}_#{userId}")

	toolTip.init(
		$takeAction,
		"TakeActionToolTip",
		"Click here to request the game.",
		"You get the idea...<br/>Request game.",
		$game.offset().left + 100
		$takeAction.offset().top
	)


$wireUpNotInterested = ($game) ->
	game = $game.game

	userId = game.Owner.Id

	$closeLink = $game.find("#closeLink#{game.Id}_#{userId}")

	$closeLink.click(-> 
		$.post(preferred.urls.notInterestedUrl, 
		{ gameId: game.Id }, 
		-> $game.fadeOut()))

	toolTip.init(
		$closeLink,
		"CloseLinkToolTip",
		"Not interested?<br/>Click to remove it.",
		"You get the idea...<br/>Remove game.",
		$game.offset().left + 100,
		$game.offset().top + -25
	)
		
$gameElementFor = (game) ->
	searchString =
		"http://www.google.com/search?q=" +
		encodeURIComponent(game.Name + " ") +
		"site:gamespot.com&btnI=3564"
	
	userId = game.Owner.Id

	gameName = game.Name

	gameName = game.Name.substring(0, 45) + "..." if game.Name.length > 45

	$game = $.tmpl gameTemplate, { gameId: game.Id, gameName, searchString, owner: game.Owner.Handle, userId }
	
	$game.game = game

	$game

this.preferred =
	init: (urls) ->
		initView()

		@urls = urls

		@getPreferredGames()

	getPreferredGames: ->
		$.getJSON(
			@urls.preferredGamesUrl,
			(games) ->
				$preferredGames.html ''
				$addGameToPage(game) for game in games
				$preferredGames.append $("<div />").css(clear: "both")

				if(!games.length)
					$preferredGames.html("Games you don't own (that your friends have) will show up here.")
			)


gameTemplate =
	'
	<div id="game${gameId}_${userId}" class="border dropshadow" style="float: left; width: 100px; height: 160px">
		<div style="padding-bottom: 5px; margin-bottom: 10px; border-bottom: 1px silver solid">
			<a href="javascript:;" id="closeLink${gameId}_${userId}"
				style="text-decoration: none; color: black; padding: 3px 15px 3px 3px; margin-left: 80px;" 
				class="cancel">&nbsp;</a>
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