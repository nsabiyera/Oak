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
    name: ko.observable(""),
    errors: ko.observableArray([]),
    save: function() {
      vm.errors([]);
      $.post(App.rabbits.createRabbitUrl, JSON.parse(ko.toJSON(vm)), function(result) {
        if(result.errors) {
          vm.errors(result.errors);
        } else {
          vm.name("");
          App.rabbits.push(result);
          App.selectedRabbit(result);
        }
      });
    }
  };

  return vm;
}

function getRabbits() {
  $.getJSON(App.routes.getRabbitsUrl, function(data) {
    App.rabbits(data.rabbits);
    App.rabbits.createRabbitUrl = data.createRabbitUrl;
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
  $.getJSON(rabbit.tasksUrl, function(data) {
    App.tasks(ToTaskViewModels(data.tasks));
    App.canAddTask(true);
    App.tasks.createTaskUrl = data.createTaskUrl;
    if(data.tasks.length == 0) {
      App.loading(false);
    }
  });
}

function TaskViewModel(task) {
  if(!task) {
    task = {};
    task.rabbitId = App.selectedRabbit.id;
    task.description = "";
    task.dueDate = "";
    task.saveUrl = App.tasks.createTaskUrl;
    task.deleteUrl = null;
  }

  var vm = {
    rabbitId: ko.observable(task.rabbitId),
    description: ko.observable(task.description),
    dueDate: ko.observable(task.dueDate),
    saveUrl: ko.observable(task.saveUrl),
    deleteUrl: ko.observable(task.deleteUrl),
    canSave: ko.observable(false),
    errorsString: ko.observable(""),
    isInvalid: ko.observable(false),
    parsedDate: ko.observable(""),
    parseDate: function() {
      var date = Date.parse(vm.dueDate());

      if(date) {
        vm.parsedDate(date.getMonth() + 1 + "/" + date.getDate() + "/" + date.getFullYear());
        return;
      }

      vm.parsedDate("?");
    },
    validate: function() {
      var errors = vm.errors();
      if(errors) {
        vm.errorsString(errors.join(", "));
        vm.canSave(false);
        vm.isInvalid(true);
      } else {
        vm.errorsString("");
        vm.canSave(true);
        vm.isInvalid(false);
      }
    },
    save: function() {
      vm.dueDate(vm.parsedDate());
      $.post(vm.saveUrl(), JSON.parse(ko.toJSON(vm)), function() {
        vm.canSave(false);
      });
    },
    errors: function () {
      var result = [];

      if(!Date.parse(vm.dueDate())) {
        result.push("invalid date");
      }

      if(!vm.description()) {
        result.push("description required");
      }

      if(result.length > 0) return result;

      else return null;
    },
    determineSave: function(data, e) {
      if(e.keyCode == 13) vm.save();
    },
    isNew: function() {
      return !task.id;
    },
    destroy: function() {
      if(vm.deleteUrl) {
        $.post(vm.deleteUrl(), function() {
          App.tasks.remove(vm);
        });
      } else {
        App.tasks.remove(vm);
      }
    }
  };

  vm.description.subscribe(vm.validate);
  vm.dueDate.subscribe(vm.validate);
  vm.dueDate.subscribe(vm.parseDate);
  vm.parseDate();

  if(vm.isNew()) vm.validate();
  else vm.id = ko.observable(task.id);

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
