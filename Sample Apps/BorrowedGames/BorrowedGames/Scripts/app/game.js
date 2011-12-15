(function() {
  var gameTemplate, _base, _base2, _base3;
  var __hasProp = Object.prototype.hasOwnProperty, __extends = function(child, parent) {
    for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; }
    function ctor() { this.constructor = child; }
    ctor.prototype = parent.prototype;
    child.prototype = new ctor;
    child.__super__ = parent.prototype;
    return child;
  }, __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };
  window.BG || (window.BG = {});
  (_base = window.BG).Models || (_base.Models = {});
  (_base2 = window.BG).Views || (_base2.Views = {});
  (_base3 = window.BG).Collections || (_base3.Collections = {});
  BG.Models.Library = (function() {
    __extends(Library, Backbone.Model);
    function Library() {
      Library.__super__.constructor.apply(this, arguments);
    }
    return Library;
  })();
  BG.Collections.Libraries = (function() {
    __extends(Libraries, Backbone.Collection);
    function Libraries() {
      Libraries.__super__.constructor.apply(this, arguments);
    }
    Libraries.prototype.model = BG.Models.Library;
    Libraries.prototype.url = "/games/preferred";
    return Libraries;
  })();
  BG.Views.PreferredGamesView = (function() {
    __extends(PreferredGamesView, Backbone.View);
    function PreferredGamesView() {
      PreferredGamesView.__super__.constructor.apply(this, arguments);
    }
    PreferredGamesView.prototype.initialize = function() {
      _.bindAll(this, 'render');
      this.preferredGames = new BG.Collections.Libraries;
      this.preferredGames.bind('reset', this.render);
      return this.preferredGames.fetch();
    };
    PreferredGamesView.prototype.render = function() {
      if (this.preferredGames.length === 0) {
        $(this.el).html('\
        <div class="info" id="showFriends" style="padding-left: 30px">\
          Games you don\'t own (that your friends have) will show up here.\
        </div>\
        ');
      } else {
        $(this.el).empty();
        this.preferredGames.each(__bind(function(library) {
          var view;
          view = new BG.Views.PreferredGameView({
            model: library
          });
          return $(this.el).append(view.render().el);
        }, this));
      }
      return this;
    };
    return PreferredGamesView;
  })();
  BG.Views.PreferredGameView = (function() {
    __extends(PreferredGameView, Backbone.View);
    function PreferredGameView() {
      PreferredGameView.__super__.constructor.apply(this, arguments);
    }
    PreferredGameView.prototype.initialize = function() {
      return _.bindAll(this, 'render');
    };
    PreferredGameView.prototype.render = function() {
      var game;
      game = $.tmpl(gameTemplate, {});
      $(this.el).html(game);
      $(this.el).append($("<div />").css({
        clear: "both"
      }));
      return this;
    };
    return PreferredGameView;
  })();
  gameTemplate = '\
  <div id="game${gameId}_${userId}" class="border dropshadow" style="float: left; width: 100px; height: 160px">\
    <div style="padding-bottom: 5px; margin-bottom: 10px; border-bottom: 1px silver solid; height: 20px">\
      <a href="javascript:;" id="closeLink${gameId}_${userId}" \
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
      <a href="javascript:;" id="takeAction${gameId}_${userId}" style="font-size: 12px">request game</a>\
    </div>\
  </div>\
  ';
}).call(this);
