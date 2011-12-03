toolTipTemplate =
	'
	<div class="border dropshadow title" style="padding: 10px">{{html message}}</div>
	'

toolTips = { }

$createToolTip = (message) -> $.tmpl toolTipTemplate, { message }

this.toolTip =
	init: ($element, key, startingMessage, endingMessage, left, top) ->
		toolTips[key] = {
			startingMessage,
			endingMessage,
			currentMessage: startingMessage,
			messageCount: 0,
			disable: -> @.messageCount = 4
		} if !toolTips[key]

		$toolTip = null

		$element.click(-> 
			$toolTip.fadeOut(-> $toolTip.remove() if $toolTip) if $toolTip
			toolTips[key].disable()
		)

		$element.hover(
			->
				if toolTips[key].messageCount > 3
					$toolTip = null
					return

				toolTips[key].currentMessage = toolTips[key].endingMessage if toolTips[key].messageCount > 2

				$toolTip = $createToolTip toolTips[key].currentMessage
				
				$("body").append($toolTip)

				$toolTip.hide()
					
				$toolTip.css(
					position: "absolute"
					left: left()
					top: top())

				$toolTip.fadeIn()

				toolTips[key].messageCount++
			->
				$toolTip.fadeOut(-> $toolTip.remove() if $toolTip) if $toolTip
		)
