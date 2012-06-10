(function() {
  var ConsultantView, DashboardView, RollOff, RollOffs;

  this.dashboard = {
    init: function() {
      return this.view = new DashboardView();
    }
  };

  DashboardView = Backbone.View.extend({
    el: "#roll_offs",
    initialize: function() {
      this.rollOffs = new RollOffs();
      this.rollOffs.bind('reset', this.render, this);
      return this.refresh();
    },
    refresh: function() {
      return this.rollOffs.fetch();
    },
    render: function() {
      var consultantContainer, monthSeperator, sorted,
        _this = this;
      $(this.el).empty();
      monthSeperator = -1;
      consultantContainer = null;
      sorted = this.rollOffs;
      return sorted.each(function(consultant) {
        var currentMonth, view;
        view = new ConsultantView({
          model: consultant
        });
        currentMonth = consultant.rollOffMonth();
        if (monthSeperator !== currentMonth) {
          monthSeperator = currentMonth;
          consultantContainer = $("<ul></ul>").addClass("thumbnails").css({
            'margin-top': '10px'
          });
          $(_this.el).append(consultantContainer);
        }
        return consultantContainer.append($("<li></li>").append(view.render().el));
      });
    },
    createSeperator: function(month, year) {}
  });

  ConsultantView = Backbone.View.extend({
    render: function() {
      $(this.el).append($.tmpl(this.engageConsultant, {
        name: this.model.name()
      }));
      return this;
    },
    engageConsultant: '\
    <a href="#" class="thumbnail">\
      <img src="http://placehold.it/130x90" alt="">\
    </a>\
    <div class="well" style="margin: 3px; padding: 3px; width: 130px">\
        <div class="btn-group">\
          <a class="btn dropdown-toggle" style="width: 110px" data-toggle="dropdown" href="#">\
            Options\
            <span class="caret"></span>\
          </a>\
          <ul class="dropdown-menu">\
            <li><a data-toggle="modal" href="#myModal">consultant has been extended</li>\
            <li><a data-toggle="modal" href="#editConsultantModal">edit</a></li>\
          </ul>\
        </div>\
        <h4 style="margin: 5px">${name}</h4>\
    </div>\
    '
  });

  RollOff = Backbone.Model.extend({
    monthName: ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"],
    name: function() {
      return this.get("Name");
    },
    rollOffDate: function() {
      return new Date(this.get("RollOffDate"));
    },
    rollOffMonth: function() {
      return this.rollOffDate().getMonth();
    },
    rollOffMonthName: function() {
      return this.monthName[this.rollOffMonth()];
    }
  });

  RollOffs = Backbone.Collection.extend({
    model: RollOff,
    url: "rolloffs/list"
  });

}).call(this);
