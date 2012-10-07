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
    daysOut: function() {
      return this.get("DaysOut");
    },
    shortName: function() {
      var name;
      name = this.name();
      if (name.length > 41) {
        name = name.substring(0, 40) + "... ";
      }
      return name += " (" + this.console() + ")";
    },
    giveGame: function(callback) {
      var _this = this;
      return $.post(this.get("GiveGame"), {}, function() {
        requested.getRequestedGames();
        return callback();
      });
    },
    gameReturned: function(callback) {
      var _this = this;
      return $.post(this.get("GameReturned"), {}, function() {
        requested.getRequestedGames();
        return callback();
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
    tagName: "tr",
    events: {
      "click .check": "giveGame",
      "click .cancel": "gameReturned"
    },
    giveGame: function() {
      var el;
      el = this.el;
      return this.model.giveGame(function() {
        return $(el).find(".check").tooltip("hide");
      });
    },
    gameReturned: function() {
      var el;
      el = this.el;
      return this.model.gameReturned(function() {
        return $(el).find(".cancel").tooltip("hide");
      });
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
      var gen, requestedBy;
      gen = $.tmpl(this.canGiveGameTemplate, {
        requestedBy: this.model.requestedBy(),
        gameName: this.model.shortName()
      });
      requestedBy = this.model.requestedBy();
      gen.find(".check").tooltip({
        title: "<span style='font-size: 16px'>click this when you have given " + requestedBy + " the game, it'll start the count down for when the game needs to be returned</span>"
      });
      return gen;
    },
    genReturnGame: function() {
      var daysOut, gen;
      daysOut = this.model.daysOut();
      if (daysOut === 0) {
        daysOut = "just borrowed";
      } else {
        daysOut = daysOut.toString() + " day(s) so far";
      }
      gen = $.tmpl(this.returnGameTemplate, {
        requestedBy: this.model.requestedBy(),
        gameName: this.model.shortName(),
        daysOut: daysOut
      });
      gen.find(".cancel").tooltip({
        title: "<span style='font-size: 16px'>the game has been returned to me</span>"
      });
      return gen;
    },
    returnGameTemplate: '\
    <td>${requestedBy} is currently <span class="label label-success">borrowing</span> ${gameName} - ${daysOut}</td>\
    <td class="span2">\
        <i class="cancel icon-ok" style="cursor: pointer" href="javascript:;"></i>\
    </td>\
    ',
    canGiveGameTemplate: '\
    <td>${requestedBy} is <span class="label label-inverse">requesting</span> ${gameName}</td>\
    <td class="span2">\
        <i class="check icon-share-alt" style="cursor: pointer" href="javascript:;"></i>\
    </td>\
    '
  });

}).call(this);
