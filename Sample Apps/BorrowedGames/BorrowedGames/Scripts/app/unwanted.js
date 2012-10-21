(function() {
  var unwantedGame, unwantedGameView, unwantedGames, unwantedGamesUrl, unwantedGamesView;

  unwantedGamesUrl = "";

  this.unwanted = {
    init: function(urls) {
      unwantedGamesUrl = urls.unwantedGamesUrl;
      return this.view = new unwantedGamesView();
    },
    getUnwantedGames: function() {
      return this.view.refresh();
    }
  };

  unwantedGame = Backbone.Model.extend({
    name: function() {
      return this.get("Name");
    },
    shortName: function() {
      var name;
      name = this.name();
      if (name.length > 21) name = name.substring(0, 20) + "... ";
      return name += " (" + this.console() + ")";
    },
    console: function() {
      return this.get("Console");
    },
    undo: function(callback) {
      var _this = this;
      return $.post(this.get("UndoNotInterested"), {}, function() {
        preferred.getPreferredGames();
        _this.change();
        return callback();
      });
    }
  });

  unwantedGames = Backbone.Collection.extend({
    model: unwantedGame,
    url: function() {
      return unwantedGamesUrl;
    }
  });

  unwantedGamesView = Backbone.View.extend({
    el: "#unwantedGames",
    initialize: function() {
      this.unwantedGames = new unwantedGames();
      this.unwantedGames.bind('reset', this.render, this);
      return this.unwantedGames.fetch();
    },
    refresh: function() {
      return this.unwantedGames.fetch();
    },
    render: function() {
      var _this = this;
      $(this.el).empty();
      this.unwantedGames.each(function(game) {
        return _this.addGame(game);
      });
      return $(this.el).append($("<div />").css({
        clear: "both"
      }));
    },
    addGame: function(game) {
      var view;
      view = new unwantedGameView({
        model: game
      });
      view.render();
      return $(this.el).append(view.el);
    }
  });

  unwantedGameView = Backbone.View.extend({
    tagName: "tr",
    initialize: function() {
      return this.model.bind('change', this.apply, this);
    },
    apply: function() {
      return $(this.el).fadeOut();
    },
    events: {
      "click .cancel": "undo"
    },
    undo: function() {
      var el;
      el = this.el;
      return this.model.undo(function() {
        return $(el).fadeOut();
      });
    },
    render: function() {
      var game;
      game = $.tmpl(this.gameTemplate, {
        gameName: this.model.shortName()
      });
      $(this.el).html(game);
      game.find(".cancel").tooltip({
        title: "<span style='font-size: 16px'>remove the game from quarantine</span>"
      });
      return this;
    },
    gameTemplate: '\
    <td class="span1">\
     <span class="label label-important">quarantined</span>\
    </td>\
    <td>${gameName}</td>\
    <td class="span1"><i class="icon-arrow-up cancel" style="cursor: pointer"></i></td>\
    '
  });

}).call(this);
