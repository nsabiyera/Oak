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
    notInterested: function(callback) {
      var _this = this;
      return $.post(this.get("NotInterested"), {}, function() {
        _this.deleted = true;
        unwanted.getUnwantedGames();
        _this.change();
        return callback();
      });
    },
    wantGame: function(callback) {
      var _this = this;
      return $.post(this.get("WantGame"), {}, function() {
        _this.wanted = true;
        wanted.getWantedGames();
        _this.change();
        return callback();
      });
    },
    owner: function() {
      return this.get("Owner").Handle;
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
    tagName: "tr",
    className: '',
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
      var el;
      el = this.el;
      return this.model.notInterested(function() {
        return $(el).fadeOut();
      });
    },
    wantGame: function() {
      var el;
      el = this.el;
      return this.model.wantGame(function() {
        return $(el).fadeOut();
      });
    },
    render: function() {
      var game;
      game = $.tmpl(this.gameTemplate, {
        gameName: this.model.shortName(),
        searchString: this.model.reviewUrl(),
        owner: this.model.owner()
      });
      $(this.el).html(game);
      game.find(".cancel").tooltip({
        "title": "if you aren't interested in the game, put it into qurantine",
        "placement": "right"
      });
      game.find(".request").tooltip({
        "title": "request the game from " + this.model.owner(),
        "placement": "right"
      });
      return this;
    },
    gameTemplate: '\
      <td class="span2">\
        <div class="btn-group">\
          <a class="btn dropdown-toggle span2" data-toggle="dropdown" href="javascript:;">options <span class="caret"></span></a>\
          <ul class="dropdown-menu">\
            <li><a href="javascript:;" class="request">request game</a></li>\
            <li><a href="javascript:;" class="cancel">not interested</a></li>\
          </ul>\
        </div>\
      </td>\
      <td><a style="color: black;" href="${searchString}" target="_blank">${gameName}</a></td>\
      <td>${owner}</td>\
    '
  });

}).call(this);
