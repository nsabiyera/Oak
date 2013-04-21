var module = angular.module('myApp', []);

function RecipientsController($scope,$http) {
    $scope.url = '/rabbits';

    $scope.rabbits = [];

    $scope.tasks = [];

    $scope.rabbitSelected = function() {
      $scope.fetchTasks();
    };
    
    $scope.fetchRecipients = function() {
        $http.get($scope.url).then(function(result){
            $scope.rabbits = result.data;
        });
    };

    $scope.fetchTasks = function() {
      $http.get($scope.selectedRabbit.TasksUrl).then(function(result) {
        $scope.tasks = result.data;
      });
    };

    $scope.fetchRecipients();
}
