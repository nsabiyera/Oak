app = this

this.dashboard =
  init: ->
    @rollOffs = new RollOffsView()

    @bench = new BenchView()

    ExtendConsultantModal.render()

    EditConsultantModal.render()

    NewConsultantModal.render()

    $("#newConsultant").click( ->
      NewConsultantModal.create(new Consultant())
    )

BenchView = Backbone.View.extend
  el: "#bench"

  initialize: ->
    @bench = new Bench()

    @bench.bind 'reset', @render, @

    @refresh()

  refresh: -> @bench.fetch()
  
  render: ->
    $(@el).empty()

    consultantContainer = $("<ul></ul>").addClass("thumbnails").css({ 'margin-top': '10px' })

    $(@el).append consultantContainer

    @bench.each (consultant) =>
      view = new ConsultantView
        model: consultant
        editor: EditConsultantModal

      consultantContainer.append $("<li></li>").append(view.render().el)

RollOffsView = Backbone.View.extend
  el: "#roll_offs"

  initialize: ->
    @rollOffs = new RollOffs()

    @rollOffs.bind 'reset', @render, @

    @rollOffs.bind 'change', @render, @

    @refresh()

  refresh: -> @rollOffs.fetch()
  
  render: ->
    $(@el).empty()

    monthSeperator = -1
    yearSeperator = -1

    consultantContainer = null

    @rollOffs.each (consultant) =>
      view = new ConsultantView
        model: consultant,
        editor: EditConsultantModal
        extendEditor: ExtendConsultantModal

      currentMonth = consultant.rollOffMonth()

      currentYear = consultant.rollOffYear()

      if(monthSeperator != currentMonth || yearSeperator != currentYear)
        monthSeperator = currentMonth
        yearSeperator = currentYear
        @createSeperator consultant.rollOffMonthName(), consultant.rollOffYear()
        consultantContainer = $("<ul></ul>").addClass("thumbnails").css({ 'margin-top': '10px' })
        $(@el).append consultantContainer

      consultantContainer.append $("<li></li>").append(view.render().el)

  createSeperator: (monthName, year) ->
    $(@el).append("<hr/>")
    $(@el).append("<h3>" + monthName + " " + (1900 + year) + "</h3>")
     

ConsultantView = Backbone.View.extend
  events:
    "click .edit": "edit",
    "click .extend": "extendConsultant"

  render: ->
    imageUrl = "http://placehold.it/130x90"

    imageUrl = @model.picture() if @model.picture()

    $(@el).append $.tmpl(@engageConsultant, { name: @model.name(), imageUrl: imageUrl }) if !@model.onBench()

    $(@el).append $.tmpl(@benchConsultant, { name: @model.name(), imageUrl: imageUrl }) if @model.onBench()

    return @

  edit: -> @options.editor.edit(@model)

  extendConsultant: -> @options.extendEditor.edit(@model)

  engageConsultant:
    '
    <a class="thumbnail">
      <img src="${imageUrl}" alt="" width="130px" height="90px">
    </a>
    <div class="well" style="margin: 3px; padding: 3px; width: 130px">
        <div class="btn-group">
          <a class="btn dropdown-toggle" style="width: 110px" data-toggle="dropdown" href="javascript:;">
            Options
            <span class="caret"></span>
          </a>
          <ul class="dropdown-menu">
            <li><a href="javascript:;" class="extend">consultant has been extended</li>
            <li><a href="javascript:;" class="edit">edit</a></li>
          </ul>
        </div>
        <h4 style="margin: 5px">${name}</h4>
    </div>
    '
  benchConsultant:
    '
    <a class="thumbnail">
      <img src="${imageUrl}" alt="" width="130px" height="90px">
    </a>
    <div class="well" style="margin: 3px; padding: 3px; width: 130px">
        <div class="btn-group">
          <a class="btn dropdown-toggle" style="width: 110px" data-toggle="dropdown" href="javascript:;">
            Options
            <span class="caret"></span>
          </a>
          <ul class="dropdown-menu">
            <li><a href="javascript:;" class="edit">edit</a></li>
          </ul>
        </div>
        <h4 style="margin: 5px">${name}</h4>
    </div>
    '

Consultant = Backbone.Model.extend
  monthName: ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"]

  name: -> @get("Name")

  setName: (name) -> @set("Name", name)

  picture: -> @get("Picture")

  setPicture: (url) -> @set("Picture", url)

  rollOffDate: ->
    if @get("RollOffDate")
      return new Date(@get("RollOffDate"))
    else
      return null

  setRollOffDate: (date) ->
    @set("RollOffDate", date)

  rollOffMonth: -> @rollOffDate().getMonth()

  rollOffMonthName: -> @monthName[@rollOffMonth()]

  rollOffYear: ->  @rollOffDate().getYear()

  onBench: -> @get("OnBench")

  update: ->
    $.post("/consultants/update", { id: @get("Id"), name: @get("Name"), rollOffDate: @get("RollOffDate"), picture: @get("Picture") }, =>
      app.dashboard.rollOffs.refresh() #bad form, consider events
      app.dashboard.bench.refresh() #bad form, consider events
    )

  create: ->
    $.post("/consultants/create", { name: @get("Name"), rollOffDate: @get("RollOffDate") }, =>
      app.dashboard.rollOffs.refresh() #bad form, consider events
      app.dashboard.bench.refresh() #bad form, consider events
    )

  extendTil: (date) ->
    $.post("/rolloffs/extensions", { consultantId: @get("Id"), til: date }, =>
      app.dashboard.rollOffs.refresh() #bad form, consider events
      app.dashboard.bench.refresh() #bad form, consider events
    )

RollOffs = Backbone.Collection.extend
  model: Consultant

  url: "rolloffs/list"

Bench = Backbone.Collection.extend
  model: Consultant

  url: "rolloffs/bench"

EditConsultantModal = new (Backbone.View.extend
  render: ->
    @el = "#editConsultantModal"

    $(@el).find("#rollOffDate").datepicker()

    $(@el).find("#updateConsultant").click( =>
      @model.setName($(@el).find("#consultantName").val())
      @model.setRollOffDate($(@el).find("#rollOffDate").val())
      @model.setPicture($(@el).find("#picture").val())
      @model.update()
      $(@el).modal('hide')
    )

    $(@el).modal
      show: false

  edit: (consultant) ->
    @model = consultant
    $("#consultantName").val(consultant.name())
    $(@el).find("#picture").val(consultant.picture())

    if consultant.rollOffDate()
      d = consultant.rollOffDate()
      date = d.getDate()
      month = d.getMonth() + 1
      year = d.getFullYear()
      formatted = month + "/" + date + "/" + year
      $("#rollOffDate").val(formatted)
    else
      $("#rollOffDate").val("")
     
    $("#rollOffDate").datepicker("update")
    $(@el).modal('show')
  )

NewConsultantModal = new (Backbone.View.extend
  render: ->
    @el = "#newConsultantModal"

    $(@el).find("#createConsultant").click( =>
      @model.setName($(@el).find("#newConsultantName").val())
      @model.create()
      $(@el).modal('hide')
    )

    $(@el).modal
      show: false

  create: (consultant) ->
    @model = consultant
     
    $(@el).modal('show')
  )

ExtendConsultantModal = new (Backbone.View.extend
  save: ->
    @model.extendTil $("#extensionDate").val()

  render: ->
    @el = "#extendConsultantModal"
    $(@el).modal
      show: false

    view = $(@el)

    $(@el).find("#extendConsultant").click =>
      @save()
      view.modal("hide")

    $(@el).find("#extensionDate").datepicker()

  edit: (consultant) ->
    @model = consultant
    $(@el).find(".consultantName").html(@model.name())
    $(@el).modal('show')
  )
