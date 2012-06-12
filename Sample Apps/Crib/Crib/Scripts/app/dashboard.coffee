app = this

this.dashboard =
  init: ->
    @rollOffs = new RollOffsView()

    @bench = new BenchView()

    ExtendConsultantModal.render()

    EditConsultantModal.render()

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

    consultantContainer = null

    @rollOffs.each (consultant) =>
      view = new ConsultantView
        model: consultant,
        editor: EditConsultantModal
        extendEditor: ExtendConsultantModal

      currentMonth = consultant.rollOffMonth()

      if(monthSeperator != currentMonth)
        monthSeperator = currentMonth
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
    $(@el).append $.tmpl(@engageConsultant, { name: @model.name() }) if !@model.onBench()

    $(@el).append $.tmpl(@benchConsultant, { name: @model.name() }) if @model.onBench()

    return @

  edit: -> @options.editor.edit(@model)

  extendConsultant: -> @options.extendEditor.edit(@model)

  engageConsultant:
    '
    <a href="#" class="thumbnail">
      <img src="http://placehold.it/130x90" alt="">
    </a>
    <div class="well" style="margin: 3px; padding: 3px; width: 130px">
        <div class="btn-group">
          <a class="btn dropdown-toggle" style="width: 110px" data-toggle="dropdown" href="#">
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
    <a href="#" class="thumbnail">
      <img src="http://placehold.it/130x90" alt="">
    </a>
    <div class="well" style="margin: 3px; padding: 3px; width: 130px">
        <div class="btn-group">
          <a class="btn dropdown-toggle" style="width: 110px" data-toggle="dropdown" href="#">
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

  rollOffDate: ->
    if @get("RollOffDate")
      return new Date(@get("RollOffDate"))
    else
      return null

  rollOffMonth: -> @rollOffDate().getMonth()

  rollOffMonthName: -> @monthName[@rollOffMonth()]

  rollOffYear: ->  @rollOffDate().getYear()

  onBench: -> @get("OnBench")

  extendTil: (date) ->
    console.log(date)
    $.post("/rolloffs/extensions", { consultantId: @get("Id"), til: date }, =>
      app.dashboard.rollOffs.refresh()
      app.dashboard.bench.refresh()
    ) #bad form, consider events

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

    d = new Date()
    date = d.getDate()
    month = d.getMonth() + 1
    year = d.getFullYear()
    formatted = month + "/" + date + "/" + year

    $(@el).find("#updateConsultant").click( ->

      $(@el).modal('hide')
    )

    $(@el).modal
      show: false

  edit: (consultant) ->
    @model = consultant
    $("#consultantName").val(consultant.name())

    if consultant.rollOffDate()
      d = consultant.rollOffDate()
      date = d.getDate()
      month = d.getMonth() + 1
      year = d.getFullYear()
      formatted = month + "/" + date + "/" + year
      $("#rollOffDate").val(formatted)
     
    $(@el).modal('show')
  )

ExtendConsultantModal = new (Backbone.View.extend
  save: ->
    @model.extendTil $("#extensionDate").val()

  render: ->
    @el = "#extendConsultantModal"
    $(@el).modal
      show: false
    $(@el).find("#extendConsultant").click => @save()
    $(@el).find("#extensionDate").datepicker()

  edit: (consultant) ->
    @model = consultant
    $(@el).find(".consultantName").html(@model.name())
    $(@el).modal('show')
  )
