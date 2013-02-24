window.Todos = Ember.Application.create({
  rootElement: '#dashboard'  
});

Todos.CreateTodoView = Em.TextField.extend({
  insertNewline: function() {
    var value = this.get('value');

    if (value) {
      Todos.todosController.createTodo(value);
      this.set('value', '');
    }
  }
});
