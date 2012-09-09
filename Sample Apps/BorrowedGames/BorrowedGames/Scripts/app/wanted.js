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
    daysLeft: function() {
      return this.get("DaysLeft");
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
      game.find(".cancel").tooltip({
        title: "<span style='font-size: 16px'>cancel game request</span>"
      });
      return $(this.el).html(game);
    },
    renderBorrowedGame: function() {
      var daysLeft, daysLeftClass, daysLeftText, game;
      daysLeft = this.model.daysLeft();
      daysLeftClass = "label-info";
      daysLeftText = daysLeft + " day(s) left";
      if (daysLeft <= 10) daysLeftClass = "label-warning";
      if (daysLeft <= 0) daysLeftClass = "label-important";
      if (daysLeft <= 0) daysLeftText = "overdue, return game";
      game = $.tmpl(this.borrowedGameTemplate, {
        gameName: this.model.shortName(),
        owner: this.model.owner(),
        daysLeft: daysLeftText,
        daysLeftClass: daysLeftClass
      });
      game.find(".cancel").tooltip({
        title: "<span style='font-size: 16px'>the game has been returned</span>"
      });
      return $(this.el).html(game);
    },
    borrowedGameTemplate: '\
    <td class="span1">\
      <span class="label label-success">currently borrowing</span>\
      <span class="label ${daysLeftClass}">${daysLeft}</span>\
    </td>\
    <td>${gameName}</td>\
    <td>${owner}</td>\
    <td class="span2"><i class="icon-remove cancel" style="cursor: pointer"></i></td>\
    ',
    requestedGameTemplate: '\
    <td class="span1">\
      <span class="label label-inverse">requested</span>\
    </td>\
    <td>${gameName}</td>\
    <td>${owner}</td>\
    <td class="span2"><i class="icon-remove cancel" style="cursor: pointer">&nbsp;</i></td>\
    '
  });

}).call(this);
