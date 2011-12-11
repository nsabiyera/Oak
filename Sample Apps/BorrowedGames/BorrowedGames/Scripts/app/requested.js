(function() {
  var addGameToPage, gameTemplate, initView, requestedGames;
  requestedGames = null;
  initView = function() {
    return requestedGames = $("#requestedGames");
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
    return requestedGames.append($game);
  };
  this.requested = {
    init: function(urls) {
      initView();
      this.urls = urls;
      return this.getRequestedGames();
    },
    getRequestedGames: function() {
      return $.getJSON(this.urls.requestedGamesUrl, function(games) {
        var game, _i, _len;
        requestedGames.html('');
        requestedGames.hide();
        for (_i = 0, _len = games.length; _i < _len; _i++) {
          game = games[_i];
          addGameToPage(game);
        }
        return requestedGames.fadeIn();
      });
    }
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
