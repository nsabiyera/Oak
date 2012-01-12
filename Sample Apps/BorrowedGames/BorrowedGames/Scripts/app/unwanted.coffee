unwantedGamesUrl = ""

this.unwanted =
  init: (urls, div) ->
    unwantedGamesUrl = urls.unwantedGamesUrl

    @view = new unwantedGamesView()

    @view.initialize()

    div.html(@view.el)

  getUnWantedGames: -> @view.refresh()


