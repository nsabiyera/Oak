(function() {
  var addGameToPage, gameTemplate, initView, wantedGame, wantedGames, wantedGamesUrl, wantedGamesView;
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
  wantedGame = Backbone.Model.extend();
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
    }
  });
  initView = function() {
    return wantedGames = $("#wantedGames");
  };
  addGameToPage = function(game) {
    var $game, gameName;
    gameName = game.Name;
    if (game.Name.length > 45) {
      gameName = game.Name.substring(0, 40) + "... ";
    }
    gameName += " (" + game.Console + ")";
    $game = $.tmpl(gameTemplate, {
      gameName: gameName,
      owner: game.Owner.Handle
    });
    return wantedGames.append($game);
  };
  gameTemplate = '\
  <div class="border">\
    <span id="status${gameId}_${userId}" style="float: right; font-size: 30px; color: silver; margin-right: 10px" class="brand">\
      Requested\
    </span>\
    <div style="font-size: 20px">${gameName}</div>\
    <div>${owner}</div>\
  </div>\
  ';
}).call(this);
