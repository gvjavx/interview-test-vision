public class TodoService
{
    private readonly List<Todo> _todos = new List<Todo>();
    private readonly object _lock = new object();
    private int _nextId = 1;

    public List<Todo> GetAll()
    {
        lock (_lock)
        {
            return _todos.ToList();
        }
    }

    public Todo? GetById(int id)
    {
        lock (_lock)
        {
            return _todos.FirstOrDefault(t => t.Id == id);
        }
    }

    public Todo Add(TodoRequest request)
    {
        lock (_lock)
        {
            Todo todo = new Todo
            {
                Id = _nextId,
                Title = (request.Title ?? "").Trim(),
                IsDone = request.IsDone
            };
            _nextId++;
            _todos.Add(todo);
            return todo;
        }
    }

    public Todo? Update(int id, TodoRequest request)
    {
        lock (_lock)
        {
            int index = _todos.FindIndex(t => t.Id == id);
            if (index == -1)
            {
                return null;
            }

            Todo updatedTodo = new Todo
            {
                Id = id,
                Title = (request.Title ?? "").Trim(),
                IsDone = request.IsDone
            };
            _todos[index] = updatedTodo;
            return updatedTodo;
        }
    }

    public bool Delete(int id)
    {
        lock (_lock)
        {
            int removedCount = _todos.RemoveAll(t => t.Id == id);
            return removedCount > 0;
        }
    }
}
