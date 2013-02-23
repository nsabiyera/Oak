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
      $scope.rabbits = data.Rabbits;
      $scope.CreateRabbitUrl = data.CreateRabbitUrl;
    });

  $scope.selectedRabbit = null;

  $scope.loadTasks = function() {
    loading();
    $http({ method: 'GET', url: $scope.selectedRabbit.TasksUrl })
      .success(function(data, status, headers, config) {
        _.each(data.Tasks, function(task) { ToTaskVm(task, $http, $scope); });
        $scope.CreateTaskUrl = data.CreateTaskUrl;
        $scope.tasks = data.Tasks;
        $scope.canAddTask = true;
        if(data.Tasks.length == 0) loaded();
      });
  };

  $scope.addTask = function() {
    var task = {
      Description: "",
      DueDate: "",
      SaveUrl: $scope.CreateTaskUrl
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
  this.Name = "";
  this.Errors = [];
  this.save = function() {
    $http.post($scope.CreateRabbitUrl, this)
      .success(function(data, status, headers, config) {
        if(data.Errors) {
          _this.Errors = data.Errors;
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
    var date = Date.parse(task.DueDate);

    if(date) {
      task.ParsedDate = date.getMonth() + 1 + "/" + date.getDate() + "/" + date.getFullYear();
    } else {
      task.ParsedDate = "?";
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

    if(!task.Description) errors.push("description required");

    if(task.ParsedDate == "?") errors.push("invalid date");
    
    task.ErrorString = errors.join(", ");

    task.CanSave = errors.length == 0;

    task.HasErrors = !task.CanSave;
  };

  task.save = function() {
    if(task.CanSave) {
      task.DueDate = task.ParsedDate;
      $http.post(task.SaveUrl, task)
        .success(function(data, status, headers, config) {
          task.CanSave = false;
        });
    }
  };

  task.destroy = function() {
    if(!task.isNew()) {
      $http.post(task.DeleteUrl).success(function() {
        $scope.tasks.remove(task);
      });
    } else {
      $scope.tasks.remove(task);
    }
  };

  task.isNew = function() {
    return !task.Id;
  };

  task.HasErrors = false;

  task.ErrorString = "";

  task.CanSave = false;

  task.ParsedDate = task.DueDate;
}
