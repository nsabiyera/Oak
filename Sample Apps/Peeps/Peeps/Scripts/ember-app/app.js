window.App = Ember.Application.create({ rootElement: "#dashboard" });

App.Router.map(function() { this.resource('peeps', { path: '/' }); });

App.RESTAdapter = Ember.Adapter.extend({
  saveRecord: function(record) {
    var data, klass, rootKey,
      _this = this;
    klass = record.constructor;
    rootKey = Ember.get(klass, 'rootKey');
    data = {};
    data[rootKey] = record.toJSON();
    $.ajax("" + (Ember.get(klass, 'url')) + "/" + (record.get('id')) + ".json", {
      type: 'PUT',
      data: data
    }).then(function(data) {
      Ember.run(function() {
        return record.set('isSaving', false);
      });
    });
  },
  createRecord: function(record) {
    var data, klass, rootKey,
      _this = this;
    klass = record.constructor;
    data = record.toJSON();
    $.ajax(Ember.get(klass, 'post_url'), {
      type: 'POST',
      data: data
    }).then(function(data) {
      Ember.run(function() {
        return record.load(data);
      });
    });
  },
  deleteRecord: function(record) {
    var klass, url,
      _this = this;
    klass = record.constructor;
    url = "" + (Ember.get(klass, 'url')) + "/" + (record.get('id')) + ".json";
    $.ajax(url, {
      type: 'DELETE'
    }).then(function(data) {
      Ember.run(function() {
        return record.didDeleteRecord();
      });
    });
  },
  findAll: function(klass, recordArray) {
    var _this = this;
    $.getJSON((Ember.get(klass, 'get_url')), {}, function(data) {
      Ember.run(function() {
        return recordArray.load(klass, data);
      });
    });
  },
  find: function(record, id) {
    $.getJSON("" + (Ember.get(record.constructor, 'url')) + "/" + id + ".json", {}, function(data) {
      var _this = this;
      Ember.run(function() {
        return record.load(data);
      });
    });
  }
});

App.PeepsRoute = Ember.Route.extend({
  model: function() {
    return App.Peep.find();
  }
});

var attr = Ember.attr;

App.Peep = Ember.Model.extend({
  name: attr(),
});

App.Peep.reopenClass({
  get_url: "/home/list",
  post_url: "/home/update"
});

App.Peep.adapter = new App.RESTAdapter();

App.PeepsController = Ember.ArrayController.extend({
  saveAll: function() {

  },
  add: function() {
    debugger;

    var peep = App.Peep.create({
      name: ""
    });

    App.Peep.adapter.createRecord(peep);
  }
});
