(function() {
  var requestedGame, requestedGames, requestedGamesUrl;
  requestedGamesUrl = "";
  this.requested = {
    init: function(urls, div) {
      requestedGamesUrl = urls.requestedGamesUrl;
      this.view = new requestedGamesView();
      return div.html(this.view.el);
    }
  };
  requestedGame = Backbone.Model.extend({
    name: function() {
      return this.get("Name");
    },
    console: function() {
      return this.get("Console");
    },
    requestedBy: function() {
      return this.get("RequestedBy").Handle;
    },
    shortName: function() {
      var name;
      name = this.name();
      if (name.length > 41) {
        name = name.substring(0, 40) + "... ";
      }
      return name += " (" + this.console() + ")";
    }
  });
  requestedGames = Backbone.Collections.extend({
    model: requestedGame,
    url: function() {
      return requestedGamesUrl;
    }
  });
}).call(this);
