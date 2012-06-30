(function() {
  var requestedGame, requestedGameView, requestedGames, requestedGamesUrl, requestedGamesView;

  requestedGamesUrl = "";

  this.requested = {
    init: function(urls) {
      requestedGamesUrl = urls.requestedGamesUrl;
      return this.view = new requestedGamesView();
    },
    getRequestedGames: function() {
      return this.view.refresh();
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
    },
    giveGame: function() {
      var _this = this;
      return $.post(this.get("GiveGame"), {}, function() {
        return requested.getRequestedGames();
      });
    },
    gameReturned: function() {
      var _this = this;
      return $.post(this.get("GameReturned"), {}, function() {
        return requested.getRequestedGames();
      });
    },
    canGiveGame: function() {
      return !!this.get("GiveGame");
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
      if (this.requestedGames.length === 0) {
        $("#requestedGamesHeader").hide();
      } else {
        $("#requestedGamesHeader").show();
      }
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
      "click .check": "giveGame",
      "click .cancel": "gameReturned"
    },
    giveGame: function() {
      return this.model.giveGame();
    },
    gameReturned: function() {
      return this.model.gameReturned();
    },
    render: function() {
      var game;
      if (this.model.canGiveGame()) {
        game = this.genCanGiveTemplate();
      }
      if (!this.model.canGiveGame()) {
        game = this.genReturnGame();
      }
      $(this.el).html(game);
      return this;
    },
    genCanGiveTemplate: function() {
      return $.tmpl(this.canGiveGameTemplate, {
        requestedBy: this.model.requestedBy(),
        gameName: this.model.shortName()
      });
    },
    genReturnGame: function() {
      return $.tmpl(this.returnGameTemplate, {
        requestedBy: this.model.requestedBy(),
        gameName: this.model.shortName()
      });
    },
    returnGameTemplate: '\
    <div style="float: right; margin-top: 15px; margin-right: 20px; font-size: 20px">\
        <a class="cancel" href="javascript:;">The game has been returned</a>\
    </div>\
    <div style="width: 60%; font-size: 20px">${requestedBy} has borrowed<br /> ${gameName}</div>\
    ',
    canGiveGameTemplate: '\
    <div style="float: right; margin-top: 15px; margin-right: 20px; font-size: 20px">\
        <a class="check" href="javascript:;">I have given him the game</a>\
    </div>\
    <div style="width: 60%; font-size: 20px">${requestedBy} is requesting<br /> ${gameName}</div>\
    '
  });

}).call(this);
