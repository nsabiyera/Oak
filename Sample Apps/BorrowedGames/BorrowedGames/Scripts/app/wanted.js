(function() {
  var wantedGame, wantedGameView, wantedGames, wantedGamesUrl, wantedGamesView;
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };
  wantedGamesUrl = "";
  this.wanted = {
    init: function(urls, div) {
      wantedGamesUrl = urls.wantedGamesUrl;
      this.view = new wantedGamesView();
      this.view.initialize();
      return div.html(this.view.el);
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
    shortName: function() {
      var name;
      name = this.name();
      if (name > 45) {
        name = name.substring(0, 40) + "... ";
      }
      return name += " (" + this.console() + ")";
    }
  });
  wantedGames = Backbone.Collection.extend({
    model: wantedGame,
    url: function() {
      return wantedGamesUrl;
    }
  });
  wantedGamesView = Backbone.View.extend({
    initialize: function() {
      _.bindAll(this, 'render');
      this.wantedGames = new wantedGames();
      this.wantedGames.bind('reset', this.render);
      return this.wantedGames.fetch();
    },
    refresh: function() {
      return this.wantedGames.refresh();
    },
    render: function() {
      $(this.el).empty();
      if (this.wantedGames.length !== 0) {
        return this.wantedGames.each(__bind(function(game) {
          var view;
          view = new wantedGameView({
            model: game
          });
          view.initialize();
          view.render();
          return $(this.el).append(view.el);
        }, this));
      }
    }
  });
  wantedGameView = Backbone.View.extend({
    initialize: function() {
      return _.bindAll(this, "render");
    },
    render: function() {
      var game;
      game = $.tmpl(this.gameTemplate, {
        gameName: this.model.shortName()
      });
      $(this.el).html(game);
      return this;
    },
    gameTemplate: '\
    <div class="border">\
      <span style="float: right; font-size: 30px; color: silver; margin-right: 10px" class="brand">\
        Requested\
      </span>\
      <div style="font-size: 20px">${gameName}</div>\
      <div>${owner}</div>\
    </div>\
    '
  });
}).call(this);
