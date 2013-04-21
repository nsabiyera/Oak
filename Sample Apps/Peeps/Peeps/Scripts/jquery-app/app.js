﻿var template = "<form class='person form-inline'><input type='text' class='personname' /> <input type='button' value='save' class='save btn' /></form>";

$(function () {
  $("#saveAll").click(SaveAll);
  $("#new").click(NewPerson);
  $(".close").click(hideSaveSuccessful);
  hideSaveSuccessful();
  GetPeople();
});

function hideSaveSuccessful() {
  $("#saveSuccessful").hide();
}

function showSaveSuccessful() {
  $("#saveSuccessful").show();
}

function NewPerson() {
  var control = $(template);
  control.find("input.personname").val("");
  control.find("input.save").click(Save);
  $("#peeps").append(control);
}

function Save() {
  var element = this;
  var person = PersonFor($(this).parent());
  $.post("/home/update", person, function (data) {
    $(element).parent().attr("data-id", data.id)
  });
}

function GetPeople() {
  $("#peeps").html('');
  $.getJSON('/home/list', function (data) {
    for (var i = 0; i < data.length; i++) {
      var control = $(template);
      control.attr("data-id", data[i].id);
      control.find("input.personname").val(data[i].name);
      control.find("input.save").click(Save);
      $("#peeps").append(control);
    }
  });
}

function PersonFor(element) {
  var name = $(element).find("input.personname").val();
  var id = $(element).attr("data-id");
  var person = { name: name };
  if (id) person.id = id;
  return person;
}

function SaveAll() {
  var people = [];

  $("#peeps .person").each(function () {
    people.push(PersonFor(this));
  });

  $.ajax({
    type: 'POST',
    url: "/home/updateall",
    data: JSON.stringify(people),
    success: function () {
      hideSaveSuccessful();
      GetPeople(); 
    },
    contentType: 'application/json'
  });
}
