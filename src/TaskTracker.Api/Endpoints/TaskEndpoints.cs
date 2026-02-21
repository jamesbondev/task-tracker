using TaskTracker.Api.Data;
using TaskTracker.Api.Models;

namespace TaskTracker.Api.Endpoints;

public static class TaskEndpoints
{
    public static RouteGroupBuilder MapTaskEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tasks");

        group.MapGet("/", async (string? tag, ITaskRepository repository) =>
        {
            var tasks = await repository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(tag))
            {
                var normalizedTag = tag.Trim().ToLowerInvariant();
                tasks = tasks.Where(t => t.Tags.Contains(normalizedTag));
            }

            return Results.Ok(tasks);
        });

        group.MapGet("/overdue", async (ITaskRepository repository) =>
        {
            var tasks = await repository.GetAllAsync();
            var overdue = tasks
                .Where(t => t.DueDate.HasValue
                             && t.DueDate.Value < DateTime.UtcNow
                             && t.Status != TaskItemStatus.Done)
                .OrderBy(t => t.DueDate)
                .ToList();
            return Results.Ok(overdue);
        });

        group.MapGet("/tags", async (ITaskRepository repository) =>
        {
            var tasks = await repository.GetAllAsync();
            var tagSummary = tasks
                .SelectMany(t => t.Tags)
                .GroupBy(t => t)
                .Select(g => new { tag = g.Key, count = g.Count() })
                .OrderBy(x => x.tag)
                .ToList();
            return Results.Ok(tagSummary);
        });

        group.MapGet("/{id:int}", async (int id, ITaskRepository repository) =>
        {
            var task = await repository.GetByIdAsync(id);
            return task is not null ? Results.Ok(task) : Results.NotFound();
        });

        group.MapPost("/", async (TaskItem task, ITaskRepository repository) =>
        {
            if (string.IsNullOrWhiteSpace(task.Title))
            {
                return Results.BadRequest(new { error = "Title is required." });
            }

            var created = await repository.CreateAsync(task);
            return Results.Created($"/api/tasks/{created.Id}", created);
        });

        group.MapPut("/{id:int}", async (int id, TaskItem task, ITaskRepository repository) =>
        {
            var existing = await repository.GetByIdAsync(id);
            if (existing is null)
            {
                return Results.NotFound();
            }

            existing.Title = task.Title;
            existing.Description = task.Description;
            existing.Status = task.Status;
            existing.DueDate = task.DueDate;
            existing.Tags = task.Tags;

            var updated = await repository.UpdateAsync(existing);
            return Results.Ok(updated);
        });

        group.MapDelete("/{id:int}", async (int id, ITaskRepository repository) =>
        {
            var deleted = await repository.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        return group;
    }
}
