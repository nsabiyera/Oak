window.App = Ember.Application.create({ rootElement: "#dashboard" });

App.Router.map(function() { this.resource('peeps', { path: '/' }); });

App.PeepsRoute = Ember.Route.extend({
  model: function() {
    return App.Peep.find();
  }
});

var attr = Ember.attr;

App.Peep = Ember.Model.extend({
  name: attr()
});

App.Peep.adapter = Ember.Adapter.create({
  findAll: function(klass, recordArray) {
    $.getJSON("/home/list").then(function(data) {
      recordArray.load(klass, data);
    });
  },
  createRecord: function(record) {
    var klass = record.constructor();
    var rootKey = Ember.get(klass, '');
    var data = record.toJSON();
    $.ajax("/home/update", {
      type: 'POST',
      data: data
    }).then(function(data) {
      Ember.run(function() { record.load(data); });
    });
  }
});

App.PeepsController = Ember.ArrayController.extend({
  saveAll: function() {
    this.get('store').commit();
  },
  add: function() {
    debugger;
    var peep = App.Peep.createRecord({
      name: ""
    });
    // this throws an exception 'has no method createRecord'
    /*
       var peep = App.Peep.createRecord({
       name: ""
       });
       */
  }
});
