ko.bindingHandlers.chosen = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var args = allBindingsAccessor().chosen;
        var observableArray = args.options;
        $(element).attr('data-placeholder', args.defaultText).addClass('chzn-select');
        $(element).chosen({
            allow_single_deselect: true
        });
        var selecting = false;
        $(element).chosen().change(function (e, opt) {
            if (opt && opt.selected) {
                var match = ko.utils.arrayFirst(observableArray(), function (item) {
                    return parseInt(opt.selected) === item[args.value];
                });
                if (match) {
                    selecting = true;
                    args.selected(match);
                    selecting = false;
                }
            } else {
                if (!selecting && args.selected()) {
                    $(element).find("option").each(function () {
                        if (parseInt($(this).attr('value')) == args.selected()[args.value]) {
                            $(this).attr('selected', 'true');
                            $(element).trigger("liszt:updated");
                        }
                    });
                }
            }
        });
        args.options.subscribe(function () {
            $(element).html('');
            $(element).append("<option value=''></option>");
            ko.utils.arrayForEach(observableArray(), function (item) {
                $(element).append("<option value='" + item[args.value] + "'>" + item[args.text] + "</option>");
            });
            $(element).trigger("liszt:updated");
        });
        args.selected.subscribe(function () {
            $(element).change();
        });
    }
}

function showAddRabbit() {
    $("#newRabbitModal").modal('show');
}

function hideAddRabbit() {
    $("#newRabbitModal").modal('hide');
}

function begin() {
  App.rabbits = ko.observableArray();
  App.selectedRabbit = ko.observable();
  App.tasks = ko.observableArray();
  App.canAddTask = ko.observable(false);
  App.selectedRabbit.subscribe(loadTasks);
  App.loading = ko.observable(false);
  App.loading.subscribe(updateLoading);
  App.taskRenderComplete = function(element, task) {
    if(task == App.tasks()[App.tasks().length-1]) {
      App.loading(false);
    }
  };

  App.rabbitRenderComplete = function (element, rabbit) {
    if(rabbit == App.rabbit()[App.rabbit().length-1]) {
      alert('done');
    }
  }

  App.addTask = function() {
    App.tasks.unshift(TaskViewModel());
  };

  App.newRabbit = RabbitViewModel();

  $("#showAddRabbit").click(showAddRabbit);
  App.rabbits.subscribe(hideAddRabbit);

  ko.applyBindings(App, $("#dashboard").element);

  getRabbits();
}

function RabbitViewModel() {
  var vm = {
    Name: ko.observable(""),
    Errors: ko.observableArray([]),
    Save: function() {
      vm.Errors([]);
      $.post(App.rabbits.CreateRabbitUrl, JSON.parse(ko.toJSON(vm)), function(result) {
        if(result.Errors) {
          vm.Errors(result.Errors);
        } else {
          vm.Name("");
          App.rabbits.push(result);
          App.selectedRabbit(result);
        }
      });
    }
  };

  return vm;
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
    App.canAddTask(true);
    App.tasks.CreateTaskUrl = data.CreateTaskUrl;
    if(data.Tasks.length == 0) {
      App.loading(false);
    }
  });
}

function TaskViewModel(task) {
  if(!task) {
    task = {};
    task.RabbitId = App.selectedRabbit.Id;
    task.Description = "";
    task.DueDate = "";
    task.SaveUrl = App.tasks.CreateTaskUrl;
    task.DeleteUrl = null;
  }

  var vm = {
    RabbitId: ko.observable(task.RabbitId),
    Description: ko.observable(task.Description),
    DueDate: ko.observable(task.DueDate),
    SaveUrl: ko.observable(task.SaveUrl),
    DeleteUrl: ko.observable(task.DeleteUrl),
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
      if(e.keyCode == 13) vm.save();
    },
    isNew: function() {
      return !task.Id;
    },
    destroy: function() {
      if(vm.DeleteUrl) {
        $.post(vm.DeleteUrl(), function() {
          App.tasks.remove(vm);
        });
      } else {
        App.tasks.remove(vm);
      }
    }
  };

  vm.Description.subscribe(vm.validate);
  vm.DueDate.subscribe(vm.validate);
  vm.DueDate.subscribe(vm.parseDate);
  vm.parseDate();

  if(vm.isNew()) vm.validate();
  else vm.Id = ko.observable(task.Id);

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
