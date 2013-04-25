window.App = Ember.Application.create({ rootElement: "#dashboard" });

App.Store = DS.Store.extend({
  revision: 12,
  adapter: 'DS.RESTAdapter'
  //adapter: 'DS.BasicAdapter'
  //adapter: 'DS.FixtureAdapter'
});


App.Router.map(function() {
  this.resource('peeps', { path: '/' });
});

App.PeepsRoute = Ember.Route.extend({
  setupController: function(controller, model) {
    controller.set('content', model);
  },
  model: function() {
    return App.Peep.find();
  }
});

App.Peep = DS.Model.extend({
  name: DS.attr('string')
});


App.PeepsController = Ember.ArrayController.extend({
  saveAll: function() {
    this.get('store').commit();
  },
  add: function() {
		var peep = App.Peep.createRecord({
      name: ""
		});
  }
});

App.Peep.sync = {
  query: function (id, process) {
    $.getJSON('/home/list', function(data) {
      process(data).load();
    });
  },
  updateRecord: function(model, process) {
    $.post('/home/update', { id: model.get("id"), name: model.get("name") });
  },
  createRecord: function(model, process) {
    $.post('/home/update', { name: model.get("name") }, function(data) {
      model.set("id", data.id);
    });
  },
  findAll: function(model, process) {

  }
};
