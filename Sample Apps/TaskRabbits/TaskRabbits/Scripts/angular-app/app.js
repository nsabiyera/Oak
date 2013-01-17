var app = angular.module("App", []);

app.directive('onKeyupFn', function() {
    return function(scope, elm, attrs) {
        var keyupFn = scope.$eval(attrs.onKeyupFn);
        elm.bind('keyup', function(evt) {
            if(evt.which != 13) {
              scope.$apply(function() {
                  keyupFn.call(scope, evt.which);
              });
            }
        });
    };
});

app.directive('onEnter', function() {
    return function(scope, elm, attrs) {
        var keyupFn = scope.$eval(attrs.onKeyupFn);
        elm.bind('keyup', function(evt) {
            if(evt.which == 13) {
              scope.$apply(function() {
                  keyupFn.call(scope, evt.which);
              });
            }
        });
    };
});

app.controller('AppCtrl', function($scope, $http) {
  $http({ method: 'GET', url: '/rabbits' })
    .success(function(data, status, headers, config) {
      $scope.rabbits = data.Rabbits;
    });

  $scope.selectedRabbit = null;

  $scope.loadTasks = function() {
    $http({ method: 'GET', url: $scope.selectedRabbit.TasksUrl })
      .success(function(data, status, headers, config) {
        _.each(data.Tasks, function(task) { ToTaskVm(task, $http); });
        $scope.tasks = data.Tasks;
      });
  };

  $scope.tasks = [];
});

function ToTaskVm(task, $http) {
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
    console.log("date changed");
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
    debugger;
    $http.post(task.SaveUrl, task)
      .success(function(data, status, headers, config) {
        alert("saved");
      });
  };

  task.HasErrors = false;

  task.ErrorString = "";

  task.CanSave = false;

  task.ParsedDate = task.DueDate;
}
