(function() {
  var wantedGame, wantedGameView, wantedGames, wantedGamesUrl, wantedGamesView;

  wantedGamesUrl = "";

  this.wanted = {
    init: function(urls) {
      wantedGamesUrl = urls.wantedGamesUrl;
      return this.view = new wantedGamesView();
    },
    getWantedGames: function() {
      return this.view.refresh();
    }
  };

  wantedGame = Backbone.Model.extend({
    name: function() {
      return this.get("Name");
    },
    console: function() {
      return this.get("Console");
    },
    owner: function() {
      return this.get("Owner").Handle;
    },
    canReturnGame: function() {
      return this.get("ReturnGame");
    },
    shortName: function() {
      var name;
      name = this.name();
      if (name.length > 41) name = name.substring(0, 40) + "... ";
      return name += " (" + this.console() + ")";
    },
    undoRequest: function() {
      var _this = this;
      return $.post(this.get("DeleteWant"), {}, function() {
        preferred.getPreferredGames();
        return _this.change();
      });
    },
    returnGame: function() {
      var _this = this;
      return $.post(this.get("ReturnGame"), {}, function() {
        preferred.getPreferredGames();
        return _this.change();
      });
    }
  });

  wantedGames = Backbone.Collection.extend({
    model: wantedGame,
    url: function() {
      return wantedGamesUrl;
    }
  });

  wantedGamesView = Backbone.View.extend({
    el: "#wantedGames",
    initialize: function() {
      this.wantedGames = new wantedGames();
      this.wantedGames.bind('reset', this.render, this);
      return this.wantedGames.fetch();
    },
    refresh: function() {
      return this.wantedGames.fetch();
    },
    render: function() {
      var _this = this;
      $(this.el).empty();
      return this.wantedGames.each(function(game) {
        return _this.addGame(game);
      });
    },
    addGame: function(game) {
      var view;
      view = new wantedGameView({
        model: game
      });
      view.render();
      return $(this.el).append(view.el);
    }
  });

  wantedGameView = Backbone.View.extend({
    className: 'border',
    initialize: function() {
      return this.model.bind('change', this.apply, this);
    },
    apply: function() {
      return $(this.el).fadeOut();
    },
    events: {
      "click .cancel": "delete"
    },
    "delete": function() {
      if (!this.model.canReturnGame()) this.model.undoRequest();
      if (this.model.canReturnGame()) return this.model.returnGame();
    },
    render: function() {
      if (!this.model.canReturnGame()) this.renderRequestedGame();
      if (this.model.canReturnGame()) this.renderBorrowedGame();
      return this;
    },
    renderRequestedGame: function() {
      var game;
      game = $.tmpl(this.requestedGameTemplate, {
        gameName: this.model.shortName(),
        owner: this.model.owner()
      });
      $(this.el).html(game);
      return toolTip.init(game.find(".cancel"), "UndoRequest", "Don't want the game?<br/>Click to undo request.", "Don't want the game?<br/>Click to undo request.", function() {
        return game.find(".cancel").offset().left - 200;
      }, function() {
        return game.find(".cancel").offset().top - 75;
      });
    },
    renderBorrowedGame: function() {
      var game;
      game = $.tmpl(this.borrowedGameTemplate, {
        gameName: this.model.shortName(),
        owner: this.model.owner()
      });
      $(this.el).html(game);
      return toolTip.init(game.find(".cancel"), "ReturnGame", "All done with the game?<br/>Click to mark it as returned.", "All done with the game?<br/>Click to mark it as returned.", function() {
        return game.find(".cancel").offset().left - 225;
      }, function() {
        return game.find(".cancel").offset().top - 75;
      });
    },
    borrowedGameTemplate: '\
  <div class="menubar">\
    <a href="javascript:;"\
       style="text-decoration: none; color: black; float: right; padding-left: 15px"\
       class="cancel">&nbsp;</a>\
    <div style="clear: both">&nbsp;</div>\
  </div>\
    <span style="float: right; font-size: 30px; color: silver; margin-right: 10px" class="brand">\
      Borrowed\
    </span>\
    <div style="font-size: 20px">${gameName}</div>\
    <div>${owner}</div>\
    ',
    requestedGameTemplate: '\
  <div class="menubar">\
    <a href="javascript:;"\
       style="text-decoration: none; color: black; float: right; padding-left: 15px"\
       class="cancel">&nbsp;</a>\
    <div style="clear: both">&nbsp;</div>\
  </div>\
    <span style="float: right; font-size: 30px; color: silver; margin-right: 10px" class="brand">\
      Requested\
    </span>\
    <div style="font-size: 20px">${gameName}</div>\
    <div>${owner}</div>\
    '
  });

}).call(this);
