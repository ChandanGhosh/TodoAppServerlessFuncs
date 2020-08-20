const baseAddress = ""; // "http://localhost:7071";
var app = new Vue({
  el: "#app",
  data: {
    todos: [],
    newTask: "",
    error: undefined,
  },
  mounted: function () {
    fetch(`${baseAddress}/api/todo`)
      .then((res) => res.json())
      .then((json) => (this.todos = json))
      .catch(
        (reason) =>
          (this.error = `Failed to get todos from server: ${reason}. Make sure backend server is up and running.`)
      );
  },
  methods: {
    addTodo: function () {
      fetch(`${baseAddress}/api/todo`, {
        method: "POST",
        body: JSON.stringify({ description: this.newTask }),
      })
        .then((res) => res.json())
        .then((json) => {
          this.todos.push(json);
          this.newTask = "";
        });
    },
    updateTodo: function (todo) {
      const body = JSON.stringify({ isCompleted: todo.isCompleted });
      fetch(`${baseAddress}/api/todo/${todo.id}`, {
        method: "PUT",
        body: body,
      }).catch((err) => (this.error = `Failed to update the todo: ${err}`));
    },
    deleteTodo: function (todo) {
      console.log(todo.id);
      fetch(`${baseAddress}/api/todo/${todo.id}`, {
        method: "DELETE",
      })
        .then((_) => (this.todos = this.todos.filter((t) => t.id !== todo.id)))
        .catch((err) => (this.error = `Failed to delete the todo: ${err}`));
    },
  },
});
