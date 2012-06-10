this.dashboard =
  init: ->
    @view = new DashboardView()

DashboardView = Backbone.View.extend
  el: "#roll_offs"

  initialize: ->
    @rollOffs = new RollOffs()

    @rollOffs.bind 'reset', @render, @

    @refresh()

  refresh: ->
    @rollOffs.fetch()
  
  render: ->
    $(@el).empty()

    monthSeperator = -1

    consultantContainer = null

    sorted = @rollOffs

    sorted.each (consultant) =>
      view = new ConsultantView
        model: consultant

      currentMonth = consultant.rollOffMonth()

      if(monthSeperator != currentMonth)
        monthSeperator = currentMonth
        consultantContainer = $("<ul></ul>").addClass("thumbnails").css({ 'margin-top': '10px' })
        $(@el).append consultantContainer

      consultantContainer.append $("<li></li>").append(view.render().el)

  createSeperator: (month, year) ->
    

ConsultantView = Backbone.View.extend
  render: ->
    $(@el).append $.tmpl(@engageConsultant, { name: @model.name() })
    return @

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
            <li><a data-toggle="modal" href="#myModal">consultant has been extended</li>
            <li><a data-toggle="modal" href="#editConsultantModal">edit</a></li>
          </ul>
        </div>
        <h4 style="margin: 5px">${name}</h4>
    </div>
    '

RollOff = Backbone.Model.extend
  monthName: ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"]

  name: -> @get("Name")

  rollOffDate: -> new Date(@get("RollOffDate"))

  rollOffMonth: -> @rollOffDate().getMonth()

  rollOffMonthName: -> @monthName[@rollOffMonth()]

RollOffs = Backbone.Collection.extend
  model: RollOff
  url: "rolloffs/list"
