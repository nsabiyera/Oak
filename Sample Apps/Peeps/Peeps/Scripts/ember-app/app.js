window.App = Ember.Application.create({ rootElement: "#dashboard" });

App.Store = DS.Store.extend({
  revision: 12,
  adapter: 'DS.BasicAdapter'
});

App.Router.map(function() {
  this.resource('peeps', { path: '/' });
});

App.PeepsRoute = Ember.Route.extend({
  model: function() {
    return App.Peep.find({ });
  }
});

App.Peep = DS.Model.extend({
  name: DS.attr('name')
});

App.Peep.sync = {
  query: function (id, process) {
    $.getJSON('/home/list', function(data) {
      process(data).load();
    });
  }
};
