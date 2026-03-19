using TaskTracker.Api.Data;
using TaskTracker.Api.Models;

namespace TaskTracker.Api.Endpoints;

public static class TaskEndpoints
{
    public static RouteGroupBuilder MapTaskEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tasks");

        group.MapGet("/", async (string? tag, string? priority, ITaskRepository repository) =>
        {
            var tasks = await repository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(tag))
            {
                var normalizedTag = tag.Trim().ToLowerInvariant();
                tasks = tasks.Where(t => t.Tags.Contains(normalizedTag));
            }

            if (!string.IsNullOrWhiteSpace(priority))
            {
                var p = Enum.Parse<TaskPriority>(priority, ignoreCase: true);
                tasks = tasks.Where(t => t.Priority == p);
            }

            return Results.Ok(tasks);
        });

        group.MapGet("/search", async (string q, ITaskRepository repository) =>
        {
            var tasks = await repository.GetAllAsync();

            var results = tasks.Where(t =>
                t.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (t.Description != null && t.Description.Contains(q, StringComparison.OrdinalIgnoreCase)));

            return Results.Ok(results);
        });

        group.MapGet("/overdue", async (ITaskRepository repository) =>
        {
            var tasks = await repository.GetAllAsync();
            var overdue = tasks.Where(t => t.DueDate != null && t.DueDate < DateTime.Now && t.Status != TaskItemStatus.Done);
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

        group.MapGet("/stats", async (ITaskRepository repository) =>
        {
            var tasks = (await repository.GetAllAsync()).ToList();
            var stats = new
            {
                total = tasks.Count,
                byStatus = new
                {
                    todo = tasks.Count(t => t.Status == TaskItemStatus.Todo),
                    inProgress = tasks.Count(t => t.Status == TaskItemStatus.InProgress),
                    done = tasks.Count(t => t.Status == TaskItemStatus.Done),
                },
                byPriority = new
                {
                    critical = tasks.Count(t => t.Priority == TaskPriority.Critical),
                    high = tasks.Count(t => t.Priority == TaskPriority.High),
                    medium = tasks.Count(t => t.Priority == TaskPriority.Medium),
                    low = tasks.Count(t => t.Priority == TaskPriority.Low),
                },
                overdue = tasks.Count(t => t.DueDate != null && t.DueDate < DateTime.Now && t.Status != TaskItemStatus.Done),
            };
            return Results.Ok(stats);
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

            if (task.DueDate != null && task.DueDate < DateTime.Now)
            {
                return Results.BadRequest(new { error = "Due date cannot be in the past." });
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
            existing.Priority = task.Priority;
            existing.DueDate = task.DueDate;
            existing.Tags = task.Tags;
            existing.UpdatedAt = DateTime.UtcNow;

            var updated = await repository.UpdateAsync(existing);
            return Results.Ok(updated);
        });

        group.MapPatch("/{id:int}/priority", async (int id, TaskPriority priority, ITaskRepository repository) =>
        {
            var existing = await repository.GetByIdAsync(id);
            if (existing is null)
            {
                return Results.NotFound();
            }

            existing.Priority = priority;
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
