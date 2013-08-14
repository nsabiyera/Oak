window.App = Ember.Application.create({ rootElement: "#dashboard" });

App.Router.map(function() { this.resource('peeps', { path: '/' }); });

App.PeepsRoute = Ember.Route.extend({
  setupController: function(controller) {
    controller.getAll();
  }
});

App.Peep = Ember.Object.extend({
  attributes: ["id","name"],
});

App.PeepController = Ember.ObjectController.extend({
  save: function() {
    var peep = this.get('model');
    $.post("/home/update", peep.getProperties(peep.attributes), function(data) {
      peep.set("id", data.id);
    });
  }
});

App.PeepsController = Ember.ArrayController.extend({
  saveAll: function() {
    var _this = this;
    var peeps = _.map(this.get('content'), function(peep) {
      return peep.getProperties(peep.attributes);
    });

    $.ajax({
        type: 'POST',
        url: "/home/updateall",
        data: JSON.stringify(peeps),
        success: function() {
          _this.getAll();
        },
        contentType: 'application/json'
    });
  },
  getAll: function() {
    var controller = this;
    $.getJSON('/home/list', function (data) {
      controller.clear();
      var peeps = _.map(data, function(entry) {
        return App.Peep.create(entry);
      });
      controller.pushObjects(peeps);
    });
  },
  add: function() {
    this.pushObject(App.Peep.create({ name: "worky" }));
  },
});
