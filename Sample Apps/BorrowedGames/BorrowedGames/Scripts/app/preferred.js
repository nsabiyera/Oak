(function() {
  var libraries, library, preferredGameView, preferredGamesUrl, preferredGamesView;

  preferredGamesUrl = "";

  this.preferred = {
    init: function(urls) {
      preferredGamesUrl = urls.preferredGamesUrl;
      return this.view = new preferredGamesView();
    },
    getPreferredGames: function() {
      return this.view.refresh();
    }
  };

  library = Backbone.Model.extend({
    reviewUrl: function() {
      return "http://www.google.com/search?q=" + encodeURIComponent(this.name() + " ") + "site:gamespot.com&btnI=3564";
    },
    name: function() {
      return this.get("Name");
    },
    console: function() {
      return this.get("Console");
    },
    shortName: function() {
      var name;
      name = this.name();
      if (name.length > 41) name = name.substring(0, 40) + "... ";
      return name += " (" + this.console() + ")";
    },
    notInterested: function() {
      var _this = this;
      return $.post(this.get("NotInterested"), {}, function() {
        _this.deleted = true;
        unwanted.getUnwantedGames();
        return _this.change();
      });
    },
    wantGame: function() {
      var _this = this;
      return $.post(this.get("WantGame"), {}, function() {
        _this.wanted = true;
        wanted.getWantedGames();
        return _this.change();
      });
    },
    deleted: false,
    wanted: false
  });

  libraries = Backbone.Collection.extend({
    model: library,
    url: function() {
      return preferredGamesUrl;
    }
  });

  preferredGamesView = Backbone.View.extend({
    el: "#preferredGames",
    initialize: function() {
      this.preferredGames = new libraries();
      this.preferredGames.bind('reset', this.render, this);
      return this.preferredGames.fetch();
    },
    refresh: function() {
      return this.preferredGames.fetch();
    },
    render: function() {
      var _this = this;
      $(this.el).empty();
      this.preferredGames.each(function(library) {
        var view;
        view = new preferredGameView({
          model: library
        });
        return $(_this.el).append(view.render().el);
      });
      $(this.el).append($("<div />").css({
        clear: "both"
      }));
      if (this.preferredGames.length === 0) {
        return $(this.el).html('\
        <div class="info" id="showFriends" style="padding-left: 30px">\
          Games you don\'t own (that your friends have) will show up here.\
        </div>\
        ');
      }
    }
  });

  preferredGameView = Backbone.View.extend({
    className: 'gameBox',
    initialize: function() {
      return this.model.bind('change', this.apply, this);
    },
    apply: function() {
      if (this.model.deleted || this.model.wanted) return $(this.el).fadeOut();
    },
    events: {
      "click .cancel": "notInterested",
      "click .request": "wantGame"
    },
    notInterested: function() {
      return this.model.notInterested();
    },
    wantGame: function() {
      return this.model.wantGame();
    },
    render: function() {
      var game;
      game = $.tmpl(this.gameTemplate, {
        gameName: this.model.shortName(),
        searchString: this.model.reviewUrl()
      });
      $(this.el).html(game);
      toolTip.init(game.find(".request"), "WantGame", "Click here to request the game.", "You get the idea...<br/>Request game.", function() {
        return game.offset().left + 100;
      }, function() {
        return requestLink.offset().top;
      });
      toolTip.init(game.find(".cancel"), "NotInterested", "Not interested?<br/>Click to remove it.", "You get the idea...<br/>Remove game.", function() {
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
  });

}).call(this);
