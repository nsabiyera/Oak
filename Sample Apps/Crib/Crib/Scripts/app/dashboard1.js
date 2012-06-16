(function() {
  var Bench, BenchView, Consultant, ConsultantView, EditConsultantModal, ExtendConsultantModal, NewConsultantModal, RollOffs, RollOffsView, app;
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };
  app = this;
  this.dashboard = {
    init: function() {
      this.rollOffs = new RollOffsView();
      this.bench = new BenchView();
      ExtendConsultantModal.render();
      EditConsultantModal.render();
      NewConsultantModal.render();
      return $("#newConsultant").click(function() {
        return NewConsultantModal.create(new Consultant());
      });
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
      var consultantContainer;
      $(this.el).empty();
      consultantContainer = $("<ul></ul>").addClass("thumbnails").css({
        'margin-top': '10px'
      });
      $(this.el).append(consultantContainer);
      return this.bench.each(__bind(function(consultant) {
        var view;
        view = new ConsultantView({
          model: consultant,
          editor: EditConsultantModal
        });
        return consultantContainer.append($("<li></li>").append(view.render().el));
      }, this));
    }
  });
  RollOffsView = Backbone.View.extend({
    el: "#roll_offs",
    initialize: function() {
      this.rollOffs = new RollOffs();
      this.rollOffs.bind('reset', this.render, this);
      this.rollOffs.bind('change', this.render, this);
      return this.refresh();
    },
    refresh: function() {
      return this.rollOffs.fetch();
    },
    render: function() {
      var consultantContainer, monthSeperator, yearSeperator;
      $(this.el).empty();
      monthSeperator = -1;
      yearSeperator = -1;
      consultantContainer = null;
      return this.rollOffs.each(__bind(function(consultant) {
        var currentMonth, currentYear, view;
        view = new ConsultantView({
          model: consultant,
          editor: EditConsultantModal,
          extendEditor: ExtendConsultantModal
        });
        currentMonth = consultant.rollOffMonth();
        currentYear = consultant.rollOffYear();
        if (monthSeperator !== currentMonth || yearSeperator !== currentYear) {
          monthSeperator = currentMonth;
          yearSeperator = currentYear;
          this.createSeperator(consultant.rollOffMonthName(), consultant.rollOffYear());
          consultantContainer = $("<ul></ul>").addClass("thumbnails").css({
            'margin-top': '10px'
          });
          $(this.el).append(consultantContainer);
        }
        return consultantContainer.append($("<li></li>").append(view.render().el));
      }, this));
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
      var imageUrl;
      imageUrl = "http://placehold.it/130x90";
      if (this.model.gravatar()) {
        imageUrl = this.model.gravatar();
      }
      if (!this.model.onBench()) {
        $(this.el).append($.tmpl(this.engageConsultant, {
          name: this.model.name(),
          imageUrl: imageUrl
        }));
      }
      if (this.model.onBench()) {
        $(this.el).append($.tmpl(this.benchConsultant, {
          name: this.model.name(),
          imageUrl: imageUrl
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
    <a class="thumbnail">\
      <img src="${imageUrl}" alt="" width="130px" height="90px">\
    </a>\
    <div class="well" style="margin: 3px; padding: 3px; width: 130px">\
        <div class="btn-group">\
          <a class="btn dropdown-toggle" style="width: 110px" data-toggle="dropdown" href="javascript:;">\
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
    <a class="thumbnail">\
      <img src="${imageUrl}" alt="" width="130px" height="90px">\
    </a>\
    <div class="well" style="margin: 3px; padding: 3px; width: 130px">\
        <div class="btn-group">\
          <a class="btn dropdown-toggle" style="width: 110px" data-toggle="dropdown" href="javascript:;">\
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
    setName: function(name) {
      return this.set("Name", name);
    },
    picture: function() {
      return this.get("Picture");
    },
    setPicture: function(url) {
      return this.set("Picture", url);
    },
    rollOffDate: function() {
      if (this.get("RollOffDate")) {
        return new Date(this.get("RollOffDate"));
      } else {
        return null;
      }
    },
    setRollOffDate: function(date) {
      return this.set("RollOffDate", date);
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
    },
    update: function() {
      return $.post("/consultants/update", {
        id: this.get("Id"),
        name: this.get("Name"),
        rollOffDate: this.get("RollOffDate"),
        picture: this.get("Picture")
      }, __bind(function() {
        app.dashboard.rollOffs.refresh();
        return app.dashboard.bench.refresh();
      }, this));
    },
    create: function() {
      return $.post("/consultants/create", {
        name: this.get("Name"),
        rollOffDate: this.get("RollOffDate")
      }, __bind(function() {
        app.dashboard.rollOffs.refresh();
        return app.dashboard.bench.refresh();
      }, this));
    },
    extendTil: function(date) {
      return $.post("/rolloffs/extensions", {
        consultantId: this.get("Id"),
        til: date
      }, __bind(function() {
        app.dashboard.rollOffs.refresh();
        return app.dashboard.bench.refresh();
      }, this));
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
    render: function() {
      this.el = "#editConsultantModal";
      $(this.el).find("#rollOffDate").datepicker();
      $(this.el).find("#updateConsultant").click(__bind(function() {
        this.model.setName($(this.el).find("#consultantName").val());
        this.model.setRollOffDate($(this.el).find("#rollOffDate").val());
        this.model.setPicture($(this.el).find("#picture").val());
        this.model.update();
        return $(this.el).modal('hide');
      }, this));
      return $(this.el).modal({
        show: false
      });
    },
    edit: function(consultant) {
      var d, date, formatted, month, year;
      this.model = consultant;
      $("#consultantName").val(consultant.name());
      $(this.el).find("#gravatar").val(consultant.gravatar());
      if (consultant.rollOffDate()) {
        d = consultant.rollOffDate();
        date = d.getDate();
        month = d.getMonth() + 1;
        year = d.getFullYear();
        formatted = month + "/" + date + "/" + year;
        $("#rollOffDate").val(formatted);
      } else {
        $("#rollOffDate").val("");
      }
      $("#rollOffDate").datepicker("update");
      return $(this.el).modal('show');
    }
  }));
  NewConsultantModal = new (Backbone.View.extend({
    render: function() {
      this.el = "#newConsultantModal";
      $(this.el).find("#createConsultant").click(__bind(function() {
        this.model.setName($(this.el).find("#newConsultantName").val());
        this.model.create();
        return $(this.el).modal('hide');
      }, this));
      return $(this.el).modal({
        show: false
      });
    },
    create: function(consultant) {
      this.model = consultant;
      return $(this.el).modal('show');
    }
  }));
  ExtendConsultantModal = new (Backbone.View.extend({
    save: function() {
      return this.model.extendTil($("#extensionDate").val());
    },
    render: function() {
      var view;
      this.el = "#extendConsultantModal";
      $(this.el).modal({
        show: false
      });
      view = $(this.el);
      $(this.el).find("#extendConsultant").click(__bind(function() {
        this.save();
        return view.modal("hide");
      }, this));
      return $(this.el).find("#extensionDate").datepicker();
    },
    edit: function(consultant) {
      this.model = consultant;
      $(this.el).find(".consultantName").html(this.model.name());
      return $(this.el).modal('show');
    }
  }));
}).call(this);
