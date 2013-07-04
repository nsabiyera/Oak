function begin() {
  $(".close").click(hideSaveSuccessful);
  App.peeps = ko.observableArray();
  App.saveAll = SaveAll;
  App.addPeep = AddPeep;
  ko.applyBindings(App, $("#dashboard").element);
  hideSaveSuccessful();
  GetPeeps();
}

function GetPeeps() {
  $.getJSON('/home/list', function (data) {
    App.peeps(_.map(data, ToPeepViewModel));
  });
}

function SaveAll() {
  $.ajax({
    type: 'POST',
    url: "/home/updateall",
    data: ko.toJSON(App.peeps),
    success: function () {
      showSaveSuccessful();
      GetPeeps(); 
    },
    contentType: 'application/json'
  });
}

function AddPeep() {
  App.peeps.push(ToPeepViewModel({ name: "" }));
}

function hideSaveSuccessful() { $("#saveSuccessful").hide(); }

function showSaveSuccessful() { $("#saveSuccessful").show(); }

function ToPeepViewModel(peep) {
  var vm = { name: ko.observable(peep.name) };

  if(peep.id) vm.id = ko.observable(peep.id);

  vm.save = function() {
    $.post("/home/update",
      JSON.parse(ko.toJSON(vm)),
      function (data) {
        if(!vm.id) vm.id = ko.observable(data.id);

        showSaveSuccessful();
      });
  };

  return vm;
}

$(function () {
  window.App = { };

  begin();
});
