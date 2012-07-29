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
    undoRequest: function(callback) {
      var _this = this;
      return $.post(this.get("DeleteWant"), {}, function() {
        preferred.getPreferredGames();
        _this.change();
        return callback();
      });
    },
    returnGame: function(callback) {
      var _this = this;
      return $.post(this.get("ReturnGame"), {}, function() {
        preferred.getPreferredGames();
        _this.change();
        return callback();
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
      if (this.wantedGames.length > 0) {
        $("#wantedGamesHeader").show();
      } else {
        $("#wantedGamesHeader").hide();
      }
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
    tagName: 'tr',
    className: '',
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
      var el;
      el = this.el;
      if (!this.model.canReturnGame()) {
        this.model.undoRequest(function() {
          return $(el).fadeOut();
        });
      }
      if (this.model.canReturnGame()) {
        return this.model.returnGame(function() {
          return $(el).fadeOut();
        });
      }
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
      return $(this.el).html(game);
    },
    renderBorrowedGame: function() {
      var game;
      game = $.tmpl(this.borrowedGameTemplate, {
        gameName: this.model.shortName(),
        owner: this.model.owner()
      });
      return $(this.el).html(game);
    },
    borrowedGameTemplate: '\
    <td class="span2">\
      <div class="btn-group">\
        <a class="btn dropdown-toggle span2" data-toggle="dropdown" href="javascript:;">borrowed <span class="caret"></span></a>\
        <ul class="dropdown-menu">\
          <li><a href="javascript:;" class="cancel">return game</a></li>\
        </ul>\
      </div>\
    </td>\
    <td>${gameName}</td>\
    <td>${owner}</td>\
    ',
    requestedGameTemplate: '\
    <td class="span2">\
      <div class="btn-group">\
        <a class="btn dropdown-toggle span2" data-toggle="dropdown" href="javascript:;">requested <span class="caret"></span></a>\
        <ul class="dropdown-menu">\
          <li><a href="javascript:;" class="cancel">cancel request</a></li>\
        </ul>\
      </div>\
    </td>\
    <td>${gameName}</td>\
    <td>${owner}</td>\
    '
  });

}).call(this);
