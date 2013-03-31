function loading() {
  $("#loadingModal").modal({ show: true, backdrop: false });
}

function loaded() {
  $("#loadingModal").modal('hide');
}

var app = angular.module("App", []);

app.directive('onKeyup', function() {
    return function(scope, elm, attrs) {
        var keyupFn = scope.$eval(attrs.onKeyup);
        elm.bind('keyup', function(evt) {
            if(evt.which != 13) {
              scope.$apply(function() {
                  keyupFn.call(scope, evt.which);
              });
            }
        });
    };
});

app.directive('loaded', function () {
  return function (scope, element, attrs) {
      if (scope.$last === true) {
        loaded();
      }
  };
});

app.directive("chosen", function() {
  var linker = function(scope, element, attr) {
    scope.$watch(attr["chosen"] + ".length", function() {
      element.trigger("liszt:updated");
    });
    element.attr('data-placeholder', attr["chosenLabel"]).addClass('chzn-select');
    element.chosen({ allow_single_deselect: true });
  };

  return {
    restrict: "A",
    link: linker
  };
});

app.directive('onEnter', function() {
    return function(scope, elm, attrs) {
        var enterFn = scope.$eval(attrs.onEnter);
        elm.bind('keyup', function(evt) {
            if(evt.which == 13) {
              scope.$apply(function() {
                  enterFn.call(scope, evt.which);
              });
            }
        });
    };
});

app.controller('AppCtrl', function($scope, $http) {
  $http({ method: 'GET', url: '/rabbits' })
    .success(function(data, status, headers, config) {
      $scope.rabbits = data.rabbits;
      $scope.createRabbitUrl = data.createRabbitUrl;
    });

  $scope.selectedRabbit = null;

  $scope.loadTasks = function() {
    loading();
    $http({ method: 'GET', url: $scope.selectedRabbit.tasksUrl })
      .success(function(data, status, headers, config) {
        _.each(data.tasks, function(task) { ToTaskVm(task, $http, $scope); });
        $scope.createTaskUrl = data.createTaskUrl;
        $scope.tasks = data.tasks;
        $scope.canAddTask = true;
        if(data.tasks.length == 0) loaded();
      });
  };

  $scope.addTask = function() {
    var task = {
      description: "",
      dueDate: "",
      saveUrl: $scope.createTaskUrl
    };
    ToTaskVm(task, $http, $scope);
    task.parseDate();
    task.validate();
    $scope.tasks.unshift(task);
  };

  $scope.addRabbit = function() {
    $("#newRabbitModal").modal('show');
    $scope.newRabbit = new Rabbit($scope, $http);
  };

  $scope.tasks = [];
  $scope.canAddTask = false;
});


function Rabbit($scope, $http) {
  var _this = this;
  this.name = "";
  this.errors = [];
  this.save = function() {
    $http.post($scope.createRabbitUrl, this)
      .success(function(data, status, headers, config) {
        if(data.errors) {
          _this.errors = data.errors;
        } else {
          $scope.rabbits.push(data);
          $("#newRabbitModal").modal('hide');
          $scope.selectedRabbit = data;
          $scope.loadTasks();
        }
      });
  };
}

function ToTaskVm(task, $http, $scope) {
  task.parseDate = function() {
    var date = Date.parse(task.dueDate);

    if(date) {
      task.parsedDate = date.getMonth() + 1 + "/" + date.getDate() + "/" + date.getFullYear();
    } else {
      task.parsedDate = "?";
    }
  };

  task.descriptionChanged = function() {
    task.validate();
  };

  task.dateChanged = function() {
    task.parseDate();
    task.validate();
  };

  task.validate = function() {
    var errors = [];

    if(!task.description) errors.push("description required");

    if(task.parsedDate == "?") errors.push("invalid date");
    
    task.errorString = errors.join(", ");

    task.canSave = errors.length == 0;

    task.hasErrors = !task.canSave;
  };

  task.save = function() {
    if(task.canSave) {
      task.dueDate = task.parsedDate;
      $http.post(task.saveUrl, task)
        .success(function(data, status, headers, config) {
          task.canSave = false;
        });
    }
  };

  task.destroy = function() {
    if(!task.isNew()) {
      $http.post(task.deleteUrl).success(function() {
        $scope.tasks.remove(task);
      });
    } else {
      $scope.tasks.remove(task);
    }
  };

  task.isNew = function() {
    return !task.id;
  };

  task.hasErrors = false;

  task.errorString = "";

  task.canSave = false;

  task.parsedDate = task.dueDate;
}
