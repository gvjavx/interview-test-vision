var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<TodoService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapHealthChecks("/health");

app.MapGet("/api/todos", (TodoService todoService) =>
{
    List<Todo> todos = todoService.GetAll();
    return Results.Ok(todos);
});

app.MapGet("/api/todos/{id:int}", (int id, TodoService todoService) =>
{
    Todo? todo = todoService.GetById(id);
    if (todo == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(todo);
});

app.MapPost("/api/todos", (TodoRequest request, TodoService todoService) =>
{
    IResult? validationError = ValidateTodo(request);
    if (validationError != null)
    {
        return validationError;
    }

    Todo todo = todoService.Add(request);
    return Results.Created("/api/todos/" + todo.Id, todo);
});

app.MapPut("/api/todos/{id:int}", (int id, TodoRequest request, TodoService todoService) =>
{
    IResult? validationError = ValidateTodo(request);
    if (validationError != null)
    {
        return validationError;
    }

    Todo? todo = todoService.Update(id, request);
    if (todo == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(todo);
});

app.MapDelete("/api/todos/{id:int}", (int id, TodoService todoService) =>
{
    bool deleted = todoService.Delete(id);
    if (deleted == false)
    {
        return Results.NotFound();
    }
    return Results.NoContent();
});

app.Run();

IResult? ValidateTodo(TodoRequest request)
{
    const int maxTitleLength = 200;

    if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length > maxTitleLength)
    {
        var errors = new Dictionary<string, string[]>();
        errors.Add("title", new string[] { "Title is required and must be at most " + maxTitleLength + " characters." });
        return Results.ValidationProblem(errors);
    }

    return null;
}
