window.Peep = Backbone.Model.extend({
  sync: function(method, model, options) {
    $.post("/home/update", model.toJSON(), function(data) {
      model.set("id", data.id);
      model.trigger("updateSuccessful");
    });
  }
});

window.Peeps = Backbone.Collection.extend({
  model: Peep,
  url: "/home/list",
  sync: function(method, model, options) {
    if(method == "read") Backbone.sync.apply(this, arguments);
    else {
      var _this = this;
      $.ajax({
          type: 'POST',
          url: "/home/updateall",
          data: JSON.stringify(_this.toJSON()),
          success: function() {
            _this.fetch();
            _this.trigger("allsaved");
          },
          contentType: 'application/json'
      });
    }
  }
});

window.PeepView = Backbone.Marionette.ItemView.extend({
  template: "#peepView",
  tagName: "form",
  className: "form-inline",
  events: {
    "click .save":  function() { this.model.save(); },
    "keyup .personname": function() {
      this.model.set({ "name": this.$(".personname").val() });
      this.$(".preview").html(this.$(".personname").val());
    }
  },
  initialize: function() {
    this.model.bind("updateSuccessful", App.dashboard.showSaveSuccessful);
  }
});

window.PeepsView = Backbone.Marionette.CollectionView.extend({
  el: "#peeps",
  itemView: PeepView
});

window.Dashboard = Backbone.View.extend({
  el: "#dashboard",
  events: {
    "click .close": "hideSaveSuccessful",
    "click #saveAll": function() { App.peeps.sync(); },
    "click #new": function() { App.peeps.add(new Peep()); }
  },
  initialize: function () {
    App.peeps.bind("allsaved", this.showSaveSuccessful);
    this.hideSaveSuccessful();
    App.peeps.fetch();
  },
  hideSaveSuccessful: function() { $("#saveSuccessful").hide(); }, 
  showSaveSuccessful: function() { $("#saveSuccessful").show(); }
});

$(function () {
  window.App = { };
  window.App.peeps = new Peeps();
  window.App.dashboard = new Dashboard();
  window.App.peepsView = new PeepsView({ collection: App.peeps });
});
