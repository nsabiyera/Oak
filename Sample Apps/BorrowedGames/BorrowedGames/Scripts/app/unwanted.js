(function() {
  var unwantedGame, unwantedGames, unwantedGamesUrl, unwantedGamesView;
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };
  unwantedGamesUrl = "";
  this.unwanted = {
    init: function(urls, div) {
      unwantedGamesUrl = urls.unwantedGamesUrl;
      this.view = new unwantedGamesView();
      this.view.initialize();
      return div.html(this.view.el);
    },
    getUnWantedGames: function() {
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
      if (name > 45) {
        name = name.substring(0, 40) + "... ";
      }
      return name += " (" + this.console() + ")";
    },
    undo: function() {
      return alert(this.get("UndoNotInterested"));
    }
  });
  unwantedGames = Backbone.Collection.extend({
    model: unwantedGame,
    url: function() {
      return unwantedGamesUrl;
    }
  });
  unwantedGamesView = Backbone.View.extend({
    initialize: function() {
      _.bindAll(this, 'render');
      this.unwantedGames = new unwantedGames();
      this.unwantedGames.bind('reset', this.render);
      return this.unwantedGames.fetch();
    },
    refresh: function() {
      return this.unwantedGames.fetch();
    },
    render: function() {
      $(this.el).empty();
      return this.unwantedGames.each(__bind(function(game) {
        var view;
        view = new unwantedGameView({
          model: game
        });
        view.initialize();
        return $(this.el).append(view.render().el);
      }, this));
    }
  });
  ({
    unwantedGameView: Backbone.View.extend({
      initialize: function() {
        _.bindAll(this, "render");
        return this.model.bind('change', this.apply);
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
        toolTip.init(game.find(".cancel"), "UndoUnwantedGame", "Want to give the game another shot? Remove it from quranteen.", "You get the idea...<br/>Remove it from quranteen.", function() {
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
      <a style="color: black;" href="${searchString}" target="_blank">${gameName}</a><br/>\
    </div>\
    <div style="font-size: 12px; height: 30px; padding-bottom: 3px">\
      ${owner}\
    </div>\
    <div style="padding-bottom: 5px; margin-bottom: 10px; border-top: 1px silver solid">\
      <a href="javascript:;" class="request" style="font-size: 12px">request game</a>\
    </div>\
    '
    })
  });
}).call(this);
