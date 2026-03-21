namespace TaskTracker.Api.Models;

public class TaskItem
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; }
    public int Priority { get; set; }
    public string? Assignee { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public List<string> Tags { get; set; } = [];

    public bool IsOverdue()
    {
        return DueDate != null && DueDate < DateTime.Now && Status != TaskItemStatus.Completed;
    }
}
