public class Todo
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public bool IsDone { get; set; }
}

public class TodoRequest
{
    public string? Title { get; set; }
    public bool IsDone { get; set; }
}
