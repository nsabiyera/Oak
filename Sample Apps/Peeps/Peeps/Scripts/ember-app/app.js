window.App = Ember.Application.create({ rootElement: "#dashboard" });

App.Store = DS.Store.extend({
  revision: 12,
  adapter: 'DS.MvcRestAdapter'
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
