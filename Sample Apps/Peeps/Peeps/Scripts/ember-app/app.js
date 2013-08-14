window.App = Ember.Application.create({ rootElement: "#dashboard" });

App.Router.map(function() { this.resource('peeps', { path: '/' }); });

App.PeepsController = Ember.ArrayController.extend({
  saveAll: function() {
    debugger;
  },
  add: function() {
    debugger;
  }
});
