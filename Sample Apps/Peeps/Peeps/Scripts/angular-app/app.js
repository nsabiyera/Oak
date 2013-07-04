var app = angular.module("App", []);

app.controller('AppCtrl', function($scope, $http) {
  $scope.peeps = [];

  $scope.addPeep = function() {
    $scope.peeps.push(ToPeepVm({ name: "" }, $http));
  };

  $scope.saveAll = function() {
    $http.post("/home/updateall", $scope.peeps)
      .success(function(data, status, headers, config) {
        $scope.getPeeps($scope, $http);
        showSaveSuccessful();
      });
  };

  $scope.getPeeps = function () {
    $http({ method: 'GET', url: "/home/list" })
      .success(function(data, status, headers, config) {
        $scope.peeps = _.map(data, function(peep) {
          return ToPeepVm(peep, $http);
        });
      });
  };

  hideSaveSuccessful();

  $scope.getPeeps();

  $(".close").click(hideSaveSuccessful);
});

function hideSaveSuccessful() { $("#saveSuccessful").hide(); }

function showSaveSuccessful() { $("#saveSuccessful").show(); }

function ToPeepVm(peep, $http) {
  peep.save = function() {
    $http.post("/home/update", peep)
      .success(function(data, status, headers, config) {
        peep.id = data.id;
        showSaveSuccessful();
      });
  };

  return peep;
}
