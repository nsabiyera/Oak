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
		$(@el).html(@model.get("Name") + ": " + @model.get("WantGame"))

		return this