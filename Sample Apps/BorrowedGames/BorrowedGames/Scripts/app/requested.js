(function() {
  var requestedGame, requestedGameView, requestedGames, requestedGamesUrl, requestedGamesView;

  requestedGamesUrl = "";

  this.requested = {
    init: function(urls) {
      requestedGamesUrl = urls.requestedGamesUrl;
      return this.view = new requestedGamesView();
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
      if (name.length > 41) name = name.substring(0, 40) + "... ";
      return name += " (" + this.console() + ")";
    },
    givenGame: function() {
      return alert("TODO: I have given him the game");
    }
  });

  requestedGames = Backbone.Collection.extend({
    model: requestedGame,
    url: function() {
      return requestedGamesUrl;
    }
  });

  requestedGamesView = Backbone.View.extend({
    el: "#requestedGames",
    initialize: function() {
      this.requestedGames = new requestedGames();
      this.requestedGames.bind('reset', this.render, this);
      return this.requestedGames.fetch();
    },
    refresh: function() {
      return this.requestedGames.fetch();
    },
    render: function() {
      var _this = this;
      $(this.el).empty();
      return this.requestedGames.each(function(game) {
        return _this.addGame(game);
      });
    },
    addGame: function(game) {
      var view;
      view = new requestedGameView({
        model: game
      });
      view.render();
      return $(this.el).append(view.el);
    }
  });

  requestedGameView = Backbone.View.extend({
    className: "border",
    events: {
      "click .check": "givenGame"
    },
    givenGame: function() {
      return this.model.givenGame();
    },
    render: function() {
      var game;
      game = $.tmpl(this.gameTemplate, {
        requestedBy: this.model.requestedBy(),
        gameName: this.model.shortName()
      });
      $(this.el).html(game);
      return this;
    },
    gameTemplate: '\
    <div style="float: right; margin-top: 15px; margin-right: 20px; font-size: 20px">\
        <a class="check" href="javascript:;">I have given him the game</a>\
    </div>\
    <div style="width: 60%; font-size: 20px">${requestedBy} is requesting<br /> ${gameName}</div>\
    '
  });

}).call(this);
