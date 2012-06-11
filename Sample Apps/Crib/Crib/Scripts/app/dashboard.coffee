this.dashboard =
  init: ->
    @rollOffs = new RollOffsView()

    @bench = new BenchView()

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

  rollOffDate: -> new Date(@get("RollOffDate"))

  rollOffMonth: -> @rollOffDate().getMonth()

  rollOffMonthName: -> @monthName[@rollOffMonth()]

  rollOffYear: ->  @rollOffDate().getYear()

  onBench: -> @get("OnBench")

RollOffs = Backbone.Collection.extend
  model: Consultant

  url: "rolloffs/list"

Bench = Backbone.Collection.extend
  model: Consultant

  url: "rolloffs/bench"

EditConsultantModal = new (Backbone.View.extend
  initialize: ->
    @el = "#editConsultantModal"

    $(@el).modal
      show: false

  edit: (consultant) ->
    @model = consultant
    $(@el).modal('show')
  )

ExtendConsultantModal = new (Backbone.View.extend
  initialize: ->
    @el = "#extendConsultantModal"

    $(@el).modal
      show: false

  edit: (consultant) ->
    @model = consultant
    $(@el).modal('show')
  )
