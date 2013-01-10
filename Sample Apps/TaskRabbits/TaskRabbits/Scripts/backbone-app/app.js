window.Rabbit = Backbone.Model.extend({
  idAttribute: "Id",
  sync: function(method, model, options) {
    $.post(App.rabbits.CreateRabbitUrl, model.toJSON(), function(data) {
      if(data.Errors) {
        model.Errors = data.Errors;
        options.addFailed(model);
      } else {
        options.success(model, data, options);
      }
    });
  }
});

window.Rabbits = Backbone.Collection.extend({
  model: Rabbit,
  url: function() { return App.routes.GetRabbitsUrl; },
  forId: function(id) {
    return this.find(function(rabbit) {
       return rabbit.get("Id") == id;
     });
  },
  parse: function(request) {
    this.CreateRabbitUrl = request.CreateRabbitUrl;
    return request.Rabbits;
  }
});

window.Task = Backbone.Model.extend({
  idAttribute: "Id",
  sync: function(method, model) {
    if(method == "delete") {
      $.post(model.get("DeleteUrl"), function(data) {
        model.trigger('deleteSuccessful');
      });
    } else {
      model.set("DueDate", model.parsedDate());
      $.post(model.get("SaveUrl"), model.toJSON(), function(data) {
        model.set("SaveUrl", data.SaveUrl);
        model.trigger('updateSuccessful');
      });
    }
  },
  parsedDate: function() {
    var date = Date.parse(this.get("DueDate"));

    if(date) {
      return date.getMonth() + 1 + "/" + date.getDate() + "/" + date.getFullYear();
    }

    return "?";
  },
  errors: function() {
    var result = [];

    if(!Date.parse(this.get("DueDate"))) {
      result.push("invalid date");
    }

    if(!this.get("Description")) {
      result.push("description required");
    }

    if(result.length > 0) return result;

    else return null;
  }
});

window.Tasks = Backbone.Collection.extend({
  initialize: function() {
    this.bind("add", this.setSaveUrl);
  },
  rabbit: null,
  url: function() {
    return this.rabbit.get("TasksUrl");
  },
  model: Task,
  parse: function(resp) {
    this.CreateTaskUrl = resp.CreateTaskUrl;
    return resp.Tasks;
  },
  setSaveUrl: function(model) {
    model.set("SaveUrl", this.CreateTaskUrl);
  }
});

window.TasksView = Backbone.View.extend({
  el: "#tasks",
  initialize: function () {
    _.bindAll(this, "renderTasks", "appendTask");
    App.tasks.bind("reset", this.renderTasks);
    App.tasks.bind("add", this.appendTask);
  },
  renderTasks: function () {
    App.dashboard.hideLoading();
    this.$el.html('');
    _.each(App.tasks.models, function (model) {
      this.$el.append(new TaskView({ model: model }).render());
    }, this);
  },
  appendTask: function(model) {
      var newTask = new TaskView({ model: model })
      newTask.render();
      newTask.showUpdate();
      this.$el.prepend(newTask.$el);
  }
});

window.TaskView = Backbone.View.extend({
  tagName: "tr",
  initialize: function () {
    _.bindAll(this, "render", "showSave", "saveSuccessful", "invalid", "deleteSuccessful");
    this.model.bind("updateSuccessful", this.saveSuccessful);
    this.model.bind("deleteSuccessful", this.deleteSuccessful);
  },
  events: {
    "keyup .date": "update",
    "keyup .description": "update",
    "click .btn-primary": "save",
    "click .icon-remove": "remove"
  },
  render: function () {
    this.$el.append(_.template($("#TaskViewTemplate").html(), this.model));
    return this.$el;
  },
  update: function (keyArgs) {
    this.model.set({
      "DueDate": this.$(".date").val(),
      "Description": this.$(".description").val()
    });
      
    this.$(".label-inverse").html(this.model.parsedDate());

    if(keyArgs.keyCode == 13 && !this.model.errors()) {
      this.save();
      return;
    }

    this.showUpdate();
  },
  showUpdate: function() {
    var errors = this.model.errors();
    if(errors) {
      this.invalid(errors);
    } else if(this.model.hasChanged()) {
      this.showSave();
    }
  },
  showSave: function () {
    this.$(".btn").show();
    this.$(".label-important").hide();
  },
  save: function () {
    this.model.save();
  },
  saveSuccessful: function() { 
    this.$(".date").val(this.model.get("DueDate"));
    this.$(".btn").hide();
    this.$(".label-important").hide();
  },
  invalid: function(errors) {
    this.$(".btn").hide();
    this.$(".label-important").html(errors.join(", "));
    this.$(".label-important").show();
  },
  remove: function() {
    if(this.model.isNew()) this.deleteSuccessful();
    
    else this.model.destroy();
  },
  deleteSuccessful: function() {
    this.undelegateEvents();
    this.stopListening();
    this.$el.remove();
  }
});
window.Dashboard = Backbone.View.extend({
  el: "#dashboard",
  events: {
    "change #rabbitsDropDown": "loadTasks",
    "click #addTask": "addTask",
    "click .icon-plus": "showAddRabbitDialog",
    "click #createRabbit": "createRabbit"
  },
  initialize: function () {
    _.bindAll(this, "renderRabbitsDropDown", "loadTasks", "addTask");
    App.rabbits.bind("reset", this.renderRabbitsDropDown);
    App.rabbits.bind("add", this.addToRabbitsDropDown);
    App.rabbits.fetch();
    $("#newRabbitModal").modal({ show: false });
  },
  renderRabbitsDropDown: function () {
    var options = _.template($("#RabbitDropDownTemplate").html(), { rabbits: App.rabbits.models });
    $("#rabbitsDropDown").append(options);
    $("#rabbitsDropDown").chosen();
  },
  addToRabbitsDropDown: function (model) {
    var options = _.template($("#RabbitDropDownTemplate").html(), { rabbits: [model] });
    var newEntry = $("<select>").append(options).find(".rabbit");
    $("#rabbitsDropDown").append(newEntry.attr("selected", true));
    $("#rabbitsDropDown").trigger("liszt:updated");

    //WAT
    //this line has to be before the call to $("#rabbitsDropDown").change();
    //otherwise it causes and infinite loop...yay javascript....
    $("#newRabbitModal").modal('hide');
    //END WAT
    
    $("#rabbitsDropDown").change();
  },
  loadTasks: function () {
    var rabbitId = $("#rabbitsDropDown").find("option:selected").attr("data-id");
    App.tasks.rabbit = App.rabbits.forId(rabbitId);
    App.tasks.fetch();
    this.showLoading();
    $("#addTask").show();
  },
  addTask: function() {
    App.tasks.add(new Task());
  },
  showAddRabbitDialog: function() {
    $("#newRabbitModal").modal('show');
    $("#newRabbitName").val('');
    $("#newRabbitName").focus();
    $("#newRabbitErrors").html('');
  },
  createRabbit: function() {
    App.rabbits.create(
        { Name: $("#newRabbitName").val() },
        {
          wait: true,
          addFailed: function(model) {
            $("#newRabbitErrors").html('');
            _.each(model.Errors, function(error) {
              var errorDiv = $("<div class='alert alert-error'></div>")
                .html(error.Value);
              $("#newRabbitErrors").append(errorDiv);
            });
          }
        });
  },
  showLoading: function() {
    $("#loadingModal").modal({ show: true, backdrop: false });
  },
  hideLoading: function() {
    $("#loadingModal").modal('hide');
  }
});

$(function () {
  window.App = { };
  $.getJSON('/api', function(d) {
    window.App.routes = d;
    window.App.rabbits = new Rabbits();
    window.App.tasks = new Tasks();
    window.App.tasksView = new TasksView();
    window.App.dashboard = new Dashboard();
  });
});
