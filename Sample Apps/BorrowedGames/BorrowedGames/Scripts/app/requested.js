(function() {
  var requestedGame, requestedGamesUrl;
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
    }
  });
}).call(this);
