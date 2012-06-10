(function() {
  var DashboardView;

  this.dashboard = {
    init: function() {
      return this.view = new DashboardView();
    }
  };

  DashboardView = Backbone.View.extend({
    initialize: function() {},
    refresh: function() {},
    render: function() {}
  });

}).call(this);
