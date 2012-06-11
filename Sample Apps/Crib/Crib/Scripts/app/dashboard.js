(function() {
  var Bench, BenchView, Consultant, ConsultantView, EditConsultantModal, ExtendConsultantModal, RollOffs, RollOffsView;

  this.dashboard = {
    init: function() {
      this.rollOffs = new RollOffsView();
      return this.bench = new BenchView();
    }
  };

  BenchView = Backbone.View.extend({
    el: "#bench",
    initialize: function() {
      this.bench = new Bench();
      this.bench.bind('reset', this.render, this);
      return this.refresh();
    },
    refresh: function() {
      return this.bench.fetch();
    },
    render: function() {
      var consultantContainer,
        _this = this;
      $(this.el).empty();
      consultantContainer = $("<ul></ul>").addClass("thumbnails").css({
        'margin-top': '10px'
      });
      $(this.el).append(consultantContainer);
      return this.bench.each(function(consultant) {
        var view;
        view = new ConsultantView({
          model: consultant,
          editor: EditConsultantModal
        });
        return consultantContainer.append($("<li></li>").append(view.render().el));
      });
    }
  });

  RollOffsView = Backbone.View.extend({
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
      var consultantContainer, monthSeperator,
        _this = this;
      $(this.el).empty();
      monthSeperator = -1;
      consultantContainer = null;
      return this.rollOffs.each(function(consultant) {
        var currentMonth, view;
        view = new ConsultantView({
          model: consultant,
          editor: EditConsultantModal,
          extendEditor: ExtendConsultantModal
        });
        currentMonth = consultant.rollOffMonth();
        if (monthSeperator !== currentMonth) {
          monthSeperator = currentMonth;
          _this.createSeperator(consultant.rollOffMonthName(), consultant.rollOffYear());
          consultantContainer = $("<ul></ul>").addClass("thumbnails").css({
            'margin-top': '10px'
          });
          $(_this.el).append(consultantContainer);
        }
        return consultantContainer.append($("<li></li>").append(view.render().el));
      });
    },
    createSeperator: function(monthName, year) {
      $(this.el).append("<hr/>");
      return $(this.el).append("<h3>" + monthName + " " + (1900 + year) + "</h3>");
    }
  });

  ConsultantView = Backbone.View.extend({
    events: {
      "click .edit": "edit",
      "click .extend": "extendConsultant"
    },
    render: function() {
      if (!this.model.onBench()) {
        $(this.el).append($.tmpl(this.engageConsultant, {
          name: this.model.name()
        }));
      }
      if (this.model.onBench()) {
        $(this.el).append($.tmpl(this.benchConsultant, {
          name: this.model.name()
        }));
      }
      return this;
    },
    edit: function() {
      return this.options.editor.edit(this.model);
    },
    extendConsultant: function() {
      return this.options.extendEditor.edit(this.model);
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
            <li><a href="javascript:;" class="extend">consultant has been extended</li>\
            <li><a href="javascript:;" class="edit">edit</a></li>\
          </ul>\
        </div>\
        <h4 style="margin: 5px">${name}</h4>\
    </div>\
    ',
    benchConsultant: '\
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
            <li><a href="javascript:;" class="edit">edit</a></li>\
          </ul>\
        </div>\
        <h4 style="margin: 5px">${name}</h4>\
    </div>\
    '
  });

  Consultant = Backbone.Model.extend({
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
    },
    rollOffYear: function() {
      return this.rollOffDate().getYear();
    },
    onBench: function() {
      return this.get("OnBench");
    }
  });

  RollOffs = Backbone.Collection.extend({
    model: Consultant,
    url: "rolloffs/list"
  });

  Bench = Backbone.Collection.extend({
    model: Consultant,
    url: "rolloffs/bench"
  });

  EditConsultantModal = new (Backbone.View.extend({
    initialize: function() {
      this.el = "#editConsultantModal";
      return $(this.el).modal({
        show: false
      });
    },
    edit: function(consultant) {
      this.model = consultant;
      return $(this.el).modal('show');
    }
  }));

  ExtendConsultantModal = new (Backbone.View.extend({
    initialize: function() {
      this.el = "#extendConsultantModal";
      return $(this.el).modal({
        show: false
      });
    },
    edit: function(consultant) {
      this.model = consultant;
      return $(this.el).modal('show');
    }
  }));

}).call(this);
