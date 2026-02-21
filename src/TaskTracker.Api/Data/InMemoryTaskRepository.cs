using System.Collections.Concurrent;
using TaskTracker.Api.Models;
using TaskTracker.Api.Utils;

namespace TaskTracker.Api.Data;

public class InMemoryTaskRepository : ITaskRepository
{
    private readonly ConcurrentDictionary<int, TaskItem> _tasks = new();
    private int _nextId;

    public InMemoryTaskRepository()
    {
        var seedTasks = new[]
        {
            new TaskItem
            {
                Title = "Set up project structure",
                Description = "Create the initial solution and project layout",
                Status = TaskItemStatus.Done,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                Tags = [TagUtility.CommonTags.Setup, TagUtility.CommonTags.Infrastructure]
            },
            new TaskItem
            {
                Title = "Implement API endpoints",
                Description = "Build the REST API for task management",
                Status = TaskItemStatus.InProgress,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                Tags = [TagUtility.CommonTags.Backend, TagUtility.CommonTags.Api]
            },
            new TaskItem
            {
                Title = "Write unit tests",
                Description = "Add tests for the repository and endpoints",
                Status = TaskItemStatus.Todo,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Tags = [TagUtility.CommonTags.Testing, TagUtility.CommonTags.Backend]
            }
        };

        foreach (var task in seedTasks)
        {
            var id = Interlocked.Increment(ref _nextId);
            task.Id = id;
            _tasks[id] = task;
        }
    }

    public Task<IEnumerable<TaskItem>> GetAllAsync()
    {
        var tasks = _tasks.Values.OrderBy(t => t.Id).AsEnumerable();
        return Task.FromResult(tasks);
    }

    public Task<TaskItem?> GetByIdAsync(int id)
    {
        _tasks.TryGetValue(id, out var task);
        return Task.FromResult(task);
    }

    public Task<TaskItem> CreateAsync(TaskItem task)
    {
        var id = Interlocked.Increment(ref _nextId);
        task.Id = id;
        task.CreatedAt = DateTime.UtcNow;
        task.Tags = TagUtility.NormalizeTags(task.Tags);
        _tasks[id] = task;
        return Task.FromResult(task);
    }

    public Task<TaskItem?> UpdateAsync(TaskItem task)
    {
        if (!_tasks.ContainsKey(task.Id))
        {
            return Task.FromResult<TaskItem?>(null);
        }

        task.Tags = TagUtility.NormalizeTags(task.Tags);
        _tasks[task.Id] = task;
        return Task.FromResult<TaskItem?>(task);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var removed = _tasks.TryRemove(id, out _);
        return Task.FromResult(removed);
    }
}
