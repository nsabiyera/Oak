requestedGamesUrl = ""

this.requested =
  init: (urls, div) ->
    requestedGamesUrl = urls.requestedGamesUrl

    @view = new requestedGamesView()

    div.html(@view.el)

requestedGame = Backbone.Model.extend
  name: -> @get("Name")

  console: -> @get("Console")

  requestedBy: -> @get("RequestedBy").Handle

  shortName: ->
    name = @name()
    
    name = name.substring(0, 40) + "... " if name.length > 41

    name += " (" + @console() + ")"

requestedGames = Backbone.Collections.extend
  model: requestedGame
  url: -> requestedGamesUrl
