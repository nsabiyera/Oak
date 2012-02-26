requestedGamesUrl = ""

this.requested =
  init: (urls, div) ->
    requestedGamesUrl = urls.requestedGamesUrl

    @view = new requestedGamesView()

    div.html(@view.el)

requestedGame = Backbone.Model.extend
  name: -> @get("Name")

  console: -> @get("Console")
