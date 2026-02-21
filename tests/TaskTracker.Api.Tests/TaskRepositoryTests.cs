using FluentAssertions;
using TaskTracker.Api.Data;
using TaskTracker.Api.Models;
using TaskTracker.Api.Utils;
using Xunit;

namespace TaskTracker.Api.Tests;

public class TaskRepositoryTests
{
    private readonly InMemoryTaskRepository _repository;

    public TaskRepositoryTests()
    {
        _repository = new InMemoryTaskRepository();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsSeededTasks()
    {
        var tasks = (await _repository.GetAllAsync()).ToList();

        tasks.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsTask()
    {
        var task = await _repository.GetByIdAsync(1);

        task.Should().NotBeNull();
        task!.Title.Should().Be("Set up project structure");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        var task = await _repository.GetByIdAsync(999);

        task.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_AddsTaskAndAssignsId()
    {
        var newTask = new TaskItem
        {
            Title = "New task",
            Description = "A brand new task"
        };

        var created = await _repository.CreateAsync(newTask);

        created.Id.Should().Be(4);
        created.Title.Should().Be("New task");
        created.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var all = (await _repository.GetAllAsync()).ToList();
        all.Should().HaveCount(4);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingTask_UpdatesSuccessfully()
    {
        var task = await _repository.GetByIdAsync(1);
        task.Should().NotBeNull();

        task!.Title = "Updated title";
        task.Status = TaskItemStatus.InProgress;

        var updated = await _repository.UpdateAsync(task);

        updated.Should().NotBeNull();
        updated!.Title.Should().Be("Updated title");
        updated.Status.Should().Be(TaskItemStatus.InProgress);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ReturnsNull()
    {
        var task = new TaskItem
        {
            Id = 999,
            Title = "Does not exist"
        };

        var result = await _repository.UpdateAsync(task);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_RemovesTask()
    {
        var deleted = await _repository.DeleteAsync(1);

        deleted.Should().BeTrue();

        var task = await _repository.GetByIdAsync(1);
        task.Should().BeNull();

        var all = (await _repository.GetAllAsync()).ToList();
        all.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ReturnsFalse()
    {
        var deleted = await _repository.DeleteAsync(999);

        deleted.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_WithTags_StoresTags()
    {
        var newTask = new TaskItem
        {
            Title = "Tagged task",
            Tags = [TagUtility.CommonTags.Bug, TagUtility.CommonTags.Frontend]
        };

        var created = await _repository.CreateAsync(newTask);

        created.Tags.Should().BeEquivalentTo([TagUtility.CommonTags.Bug, TagUtility.CommonTags.Frontend]);
    }

    [Fact]
    public async Task CreateAsync_NormalizesTags_ToLowercaseTrimmedAndDeduplicated()
    {
        var newTask = new TaskItem
        {
            Title = "Normalization test",
            Tags = ["  BUG  ", "Bug", "FRONTEND", "frontend", "  ", ""]
        };

        var created = await _repository.CreateAsync(newTask);

        created.Tags.Should().BeEquivalentTo([TagUtility.CommonTags.Bug, TagUtility.CommonTags.Frontend]);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesTagsOnExistingTask()
    {
        var task = await _repository.GetByIdAsync(1);
        task.Should().NotBeNull();

        task!.Tags = [TagUtility.CommonTags.Urgent, TagUtility.CommonTags.Reviewed];
        var updated = await _repository.UpdateAsync(task);

        updated.Should().NotBeNull();
        updated!.Tags.Should().BeEquivalentTo([TagUtility.CommonTags.Urgent, TagUtility.CommonTags.Reviewed]);
    }

    [Fact]
    public async Task UpdateAsync_NormalizesTagsOnUpdate()
    {
        var task = await _repository.GetByIdAsync(1);
        task.Should().NotBeNull();

        task!.Tags = ["  URGENT  ", "urgent", ""];
        var updated = await _repository.UpdateAsync(task);

        updated.Should().NotBeNull();
        updated!.Tags.Should().BeEquivalentTo([TagUtility.CommonTags.Urgent]);
    }

    [Fact]
    public async Task SeededTasks_HaveTags()
    {
        var task1 = await _repository.GetByIdAsync(1);
        var task2 = await _repository.GetByIdAsync(2);
        var task3 = await _repository.GetByIdAsync(3);

        task1!.Tags.Should().BeEquivalentTo([TagUtility.CommonTags.Setup, TagUtility.CommonTags.Infrastructure]);
        task2!.Tags.Should().BeEquivalentTo([TagUtility.CommonTags.Backend, TagUtility.CommonTags.Api]);
        task3!.Tags.Should().BeEquivalentTo([TagUtility.CommonTags.Testing, TagUtility.CommonTags.Backend]);
    }

    [Fact]
    public async Task GetAllAsync_FilterByTag_ReturnsMatchingTasks()
    {
        var tasks = (await _repository.GetAllAsync())
            .Where(t => t.Tags.Contains(TagUtility.CommonTags.Backend))
            .ToList();

        tasks.Should().HaveCount(2);
        tasks.Select(t => t.Title).Should().Contain("Implement API endpoints");
        tasks.Select(t => t.Title).Should().Contain("Write unit tests");
    }

    [Fact]
    public async Task GetAllAsync_FilterByTag_NoMatches_ReturnsEmpty()
    {
        var tasks = (await _repository.GetAllAsync())
            .Where(t => t.Tags.Contains("nonexistent"))
            .ToList();

        tasks.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_TagsSummary_ReturnsDistinctTagsWithCounts()
    {
        var tasks = (await _repository.GetAllAsync()).ToList();
        var tagSummary = tasks
            .SelectMany(t => t.Tags)
            .GroupBy(t => t)
            .Select(g => new { tag = g.Key, count = g.Count() })
            .OrderBy(x => x.tag)
            .ToList();

        tagSummary.Should().HaveCount(5);
        tagSummary.Should().Contain(x => x.tag == TagUtility.CommonTags.Backend && x.count == 2);
        tagSummary.Should().Contain(x => x.tag == TagUtility.CommonTags.Api && x.count == 1);
        tagSummary.Should().Contain(x => x.tag == TagUtility.CommonTags.Setup && x.count == 1);
        tagSummary.Should().Contain(x => x.tag == TagUtility.CommonTags.Infrastructure && x.count == 1);
        tagSummary.Should().Contain(x => x.tag == TagUtility.CommonTags.Testing && x.count == 1);
    }

    [Fact]
    public void TaskItem_Tags_DefaultsToEmptyList()
    {
        var task = new TaskItem { Title = "No tags" };

        task.Tags.Should().NotBeNull();
        task.Tags.Should().BeEmpty();
    }

    [Fact]
    public void TaskItem_DueDate_DefaultsToNull()
    {
        var task = new TaskItem { Title = "No due date" };

        task.DueDate.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithDueDate_StoresDueDate()
    {
        var dueDate = DateTime.UtcNow.AddDays(7);
        var newTask = new TaskItem
        {
            Title = "Task with due date",
            DueDate = dueDate
        };

        var created = await _repository.CreateAsync(newTask);

        created.DueDate.Should().Be(dueDate);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesDueDateOnExistingTask()
    {
        var task = await _repository.GetByIdAsync(1);
        task.Should().NotBeNull();

        var newDueDate = DateTime.UtcNow.AddDays(14);
        task!.DueDate = newDueDate;
        var updated = await _repository.UpdateAsync(task);

        updated.Should().NotBeNull();
        updated!.DueDate.Should().Be(newDueDate);
    }

    [Fact]
    public async Task UpdateAsync_CanClearDueDate()
    {
        var task = await _repository.GetByIdAsync(2);
        task.Should().NotBeNull();
        task!.DueDate.Should().NotBeNull();

        task.DueDate = null;
        var updated = await _repository.UpdateAsync(task);

        updated.Should().NotBeNull();
        updated!.DueDate.Should().BeNull();
    }

    [Fact]
    public async Task SeededTasks_HaveDueDates()
    {
        var task1 = await _repository.GetByIdAsync(1);
        var task2 = await _repository.GetByIdAsync(2);
        var task3 = await _repository.GetByIdAsync(3);

        task1!.DueDate.Should().NotBeNull();
        task2!.DueDate.Should().NotBeNull();
        task3!.DueDate.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllAsync_OverdueTasks_ReturnsOnlyIncompleteTasksPastDueDate()
    {
        var tasks = (await _repository.GetAllAsync()).ToList();

        var overdue = tasks
            .Where(t => t.DueDate.HasValue
                         && t.DueDate.Value < DateTime.UtcNow
                         && t.Status != TaskItemStatus.Done)
            .ToList();

        // Task 1 is Done (not overdue even though past due), Task 3 is Todo and past due
        overdue.Should().HaveCount(1);
        overdue[0].Title.Should().Be("Write unit tests");
    }

    [Fact]
    public async Task GetAllAsync_OverdueTasks_ExcludesCompletedTasks()
    {
        var tasks = (await _repository.GetAllAsync()).ToList();

        var overdue = tasks
            .Where(t => t.DueDate.HasValue
                         && t.DueDate.Value < DateTime.UtcNow
                         && t.Status != TaskItemStatus.Done)
            .ToList();

        overdue.Should().NotContain(t => t.Status == TaskItemStatus.Done);
    }

    [Fact]
    public async Task GetAllAsync_OverdueTasks_ExcludesTasksWithFutureDueDate()
    {
        var tasks = (await _repository.GetAllAsync()).ToList();

        var overdue = tasks
            .Where(t => t.DueDate.HasValue
                         && t.DueDate.Value < DateTime.UtcNow
                         && t.Status != TaskItemStatus.Done)
            .ToList();

        overdue.Should().NotContain(t => t.Title == "Implement API endpoints");
    }

    [Fact]
    public async Task GetAllAsync_OverdueTasks_ExcludesTasksWithNoDueDate()
    {
        var newTask = new TaskItem
        {
            Title = "No due date task",
            Status = TaskItemStatus.Todo
        };
        await _repository.CreateAsync(newTask);

        var tasks = (await _repository.GetAllAsync()).ToList();
        var overdue = tasks
            .Where(t => t.DueDate.HasValue
                         && t.DueDate.Value < DateTime.UtcNow
                         && t.Status != TaskItemStatus.Done)
            .ToList();

        overdue.Should().NotContain(t => t.Title == "No due date task");
    }
}
