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
    undo: function() {
      var _this = this;
      return $.post(this.get("UndoNotInterested"), {}, function() {
        preferred.getPreferredGames();
        return _this.change();
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
    className: 'gameBoxSmall',
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
      return this.model.undo();
    },
    render: function() {
      var game;
      game = $.tmpl(this.gameTemplate, {
        gameName: this.model.shortName()
      });
      $(this.el).html(game);
      toolTip.init(game.find(".cancel"), "UndoUnwantedGame", "Want to give the game another shot?<br/>Remove it from qurantine.", "You get the idea...<br/>Remove it from qurantine.", function() {
        return game.offset().left + 100;
      }, function() {
        return game.offset().top + -25;
      });
      return this;
    },
    gameTemplate: '\
    <div class="menubar">\
      <a href="javascript:;" \
         style="text-decoration: none; color: black; float: right; padding-left: 15px" \
         class="cancel">&nbsp;</a>\
      <div style="clear: both">&nbsp;</div>\
    </div>\
    <div style="font-size: 12px; height: 70px; padding-bottom: 3px">\
      ${gameName}<br/>\
    </div>\
    '
  });

}).call(this);
