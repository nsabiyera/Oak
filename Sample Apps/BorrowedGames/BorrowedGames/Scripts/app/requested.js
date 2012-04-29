(function() {
  var requestedGame, requestedGameView, requestedGames, requestedGamesUrl, requestedGamesView;
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };
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
      return $.post(this.get("GiveGame"), {}, __bind(function() {
        return requested.getRequestedGames();
      }, this));
    },
    gameReturned: function() {
      return $.post(this.get("GameReturned"), {}, __bind(function() {
        return requested.getRequestedGames();
      }, this));
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
      $(this.el).empty();
      return this.requestedGames.each(__bind(function(game) {
        return this.addGame(game);
      }, this));
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
    <div style="width: 60%; font-size: 20px">${requestedBy} is requesting<br /> ${gameName}</div>\
    ',
    canGiveGameTemplate: '\
    <div style="float: right; margin-top: 15px; margin-right: 20px; font-size: 20px">\
        <a class="check" href="javascript:;">I have given him the game</a>\
    </div>\
    <div style="width: 60%; font-size: 20px">${requestedBy} is requesting<br /> ${gameName}</div>\
    '
  });
}).call(this);
