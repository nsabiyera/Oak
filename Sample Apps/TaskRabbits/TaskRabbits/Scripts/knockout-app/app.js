function begin() {
  App.rabbits = ko.observableArray();
  App.selectedRabbit = ko.observable();
  App.tasks = ko.observableArray();
  App.canAddTask = ko.observable(false);
  App.selectedRabbit.subscribe(loadTasks);
  App.loading = ko.observable(false);
  App.loading.subscribe(updateLoading);
  App.taskRenderComplete = finishedLoadingIfLast;
  ko.applyBindings(App, $("#dashboard").element);
  getRabbits();
}

function finishedLoadingIfLast(element, task) {
  if(task == App.tasks()[App.tasks().length-1]) {
    App.loading(false);
  }
}

function getRabbits() {
  $.getJSON(App.routes.GetRabbitsUrl, function(data) {
    App.rabbits(data.Rabbits);
    App.rabbits.CreateRabbitUrl = data.CreateRabbitUrl;
  });
}

function updateLoading() {
  if(App.loading()) {
    $("#loadingModal").modal({ show: true, backdrop: false });
  } else {
    $("#loadingModal").modal('hide');
  }
}

function loadTasks(rabbit) {
  App.loading(true);
  $.getJSON(rabbit.TasksUrl, function(data) {
    App.tasks(ToTaskViewModels(data.Tasks));
    console.log(App.lastTask);
    App.tasks.CreateTaskUrl = data.CreateTaskUrl;
  });
}

function TaskViewModel(task) {
  var vm = {
    Id: ko.observable(task.Id),
    RabbitId: ko.observable(task.RabbitId),
    Description: ko.observable(task.Description),
    DueDate: ko.observable(task.DueDate),
    SaveUrl: ko.observable(task.SaveUrl),
    CanSave: ko.observable(false),
    ErrorsString: ko.observable(""),
    IsInvalid: ko.observable(false),
    ParsedDate: ko.observable(""),
    parseDate: function() {
      var date = Date.parse(vm.DueDate());

      if(date) {
        vm.ParsedDate(date.getMonth() + 1 + "/" + date.getDate() + "/" + date.getFullYear());
        return;
      }

      vm.ParsedDate("?");
    },
    validate: function() {
      var errors = vm.errors();
      if(errors) {
        vm.ErrorsString(errors.join(", "));
        vm.CanSave(false);
        vm.IsInvalid(true);
      } else {
        vm.ErrorsString("");
        vm.CanSave(true);
        vm.IsInvalid(false);
      }
    },
    save: function() {
      vm.DueDate(vm.ParsedDate());
      $.post(vm.SaveUrl(), JSON.parse(ko.toJSON(vm)), function() {
        vm.CanSave(false);
      });
    },
    errors: function () {
      var result = [];

      if(!Date.parse(vm.DueDate())) {
        result.push("invalid date");
      }

      if(!vm.Description()) {
        result.push("description required");
      }

      if(result.length > 0) return result;

      else return null;
    },
    determineSave: function(data, e) {
      if(e.keyCode == 13) {
        vm.save();
      }
    }
  };

  vm.Description.subscribe(vm.validate);
  vm.DueDate.subscribe(vm.validate);
  vm.DueDate.subscribe(vm.parseDate);
  vm.parseDate();

  return vm;
}

function ToTaskViewModels(tasks) {
  var results = [];
  _.each(tasks, function(task) {
    results.push(TaskViewModel(task));
  });

  return results;
}

$(function () {
  window.App = { };
  $.getJSON('/api', function(d) {
    window.App.routes = d;
    begin();
  });
});
