using NUnit.Framework;

namespace ToDoManagementSystem.E2ETests.Tasks;

[TestFixture]
public class TaskCrudTests : PlaywrightTestBase
{
    [SetUp]
    public async Task SetUp()
    {
        await RegisterAndLoginAsync();
        await Page.GotoAsync($"{BaseUrl}/tasks");
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
    }

    // ─── Helpers ────────────────────────────────────────────────────────────────
    private async Task OpenCreateModalAsync()
    {
        await Page.ClickAsync("button:has-text('New Task')");
        await Page.WaitForSelectorAsync("#taskModal.show", new() { Timeout = 5_000 });
    }

    private async Task FillAndSaveTaskAsync(string title, string priority = "1", string status = "0", int dueDaysFromNow = 3)
    {
        await Page.FillAsync("#taskTitle", title);
        await Page.SelectOptionAsync("#taskPriority", priority);
        await Page.SelectOptionAsync("#taskStatus", status);
        string dueDate = DateTime.UtcNow.AddDays(dueDaysFromNow).ToString("yyyy-MM-dd");
        await Page.FillAsync("#taskDueDate", dueDate);
        await Page.ClickAsync("#saveBtn");
        await Page.WaitForSelectorAsync("#taskModal:not(.show)", new() { Timeout = 8_000 });
    }

    // ─── Task list page ─────────────────────────────────────────────────────────
    [Test]
    public async Task TasksPage_Loads_ShowsFilterControls()
    {
        await Expect(Page.Locator("#filterStatus")).ToBeVisibleAsync();
        await Expect(Page.Locator("#filterPriority")).ToBeVisibleAsync();
        await Expect(Page.Locator("#filterSearch")).ToBeVisibleAsync();
    }

    [Test]
    public async Task TasksPage_NewTaskButton_IsVisible()
    {
        await Expect(Page.Locator("button:has-text('New Task')")).ToBeVisibleAsync();
    }

    // ─── Create ─────────────────────────────────────────────────────────────────
    [Test]
    public async Task CreateTask_Modal_OpensWithLowPriorityDefault()
    {
        await OpenCreateModalAsync();
        string priorityVal = await Page.Locator("#taskPriority").InputValueAsync();
        Assert.That(priorityVal, Is.EqualTo("1"), "New task modal should default to Low priority");
    }

    [Test]
    public async Task CreateTask_WithLowPriority_SavesAsLow()
    {
        await OpenCreateModalAsync();
        await FillAndSaveTaskAsync("Low Priority Task", priority: "1");

        // Reload and confirm badge
        await Page.WaitForSelectorAsync("#tasksBody tr", new() { Timeout = 5_000 });
        string badgeText = await Page.Locator(".badge-priority-low").First.InnerTextAsync();
        Assert.That(badgeText, Is.EqualTo("Low"));
    }

    [Test]
    public async Task CreateTask_WithHighPriority_SavesAsHigh()
    {
        await OpenCreateModalAsync();
        await FillAndSaveTaskAsync("High Priority Task", priority: "3");

        await Page.WaitForSelectorAsync("#tasksBody tr", new() { Timeout = 5_000 });
        string badgeText = await Page.Locator(".badge-priority-high").First.InnerTextAsync();
        Assert.That(badgeText, Is.EqualTo("High"));
    }

    [Test]
    public async Task CreateTask_WithMediumPriority_SavesAsMedium()
    {
        await OpenCreateModalAsync();
        await FillAndSaveTaskAsync("Medium Priority Task", priority: "2");

        await Page.WaitForSelectorAsync("#tasksBody tr", new() { Timeout = 5_000 });
        string badgeText = await Page.Locator(".badge-priority-medium").First.InnerTextAsync();
        Assert.That(badgeText, Is.EqualTo("Medium"));
    }

    [Test]
    public async Task CreateTask_EmptyTitle_ShowsValidationError()
    {
        await OpenCreateModalAsync();
        await Page.FillAsync("#taskTitle", "");
        await Page.ClickAsync("#saveBtn");

        // Modal stays open and shows error
        await Expect(Page.Locator("#modalAlert")).ToBeVisibleAsync();
    }

    [Test]
    public async Task CreateTask_AllStatuses_SaveCorrectly()
    {
        string[] statuses = { "0", "1", "2" };
        string[] statusLabels = { "Pending", "InProgress", "Completed" };

        for (int i = 0; i < statuses.Length; i++)
        {
            await OpenCreateModalAsync();
            await FillAndSaveTaskAsync($"Status Task {statusLabels[i]}", status: statuses[i]);
        }

        await Page.WaitForSelectorAsync("#tasksBody tr", new() { Timeout = 5_000 });
        int rowCount = await Page.Locator("#tasksBody tr").CountAsync();
        Assert.That(rowCount, Is.GreaterThanOrEqualTo(3));
    }

    // ─── Edit ───────────────────────────────────────────────────────────────────
    [Test]
    public async Task EditTask_ChangesTitle_SavesNewTitle()
    {
        await OpenCreateModalAsync();
        await FillAndSaveTaskAsync("Original Title");
        await Page.WaitForSelectorAsync("#tasksBody tr", new() { Timeout = 5_000 });

        // Click edit on first row
        await Page.Locator("#tasksBody tr").First.Locator("button.btn-outline-primary").ClickAsync();
        await Page.WaitForSelectorAsync("#taskModal.show", new() { Timeout = 5_000 });

        await Page.FillAsync("#taskTitle", "Updated Title");
        await Page.ClickAsync("#saveBtn");
        await Page.WaitForSelectorAsync("#taskModal:not(.show)", new() { Timeout = 8_000 });

        await Expect(Page.Locator("#tasksBody")).ToContainTextAsync("Updated Title");
    }

    [Test]
    public async Task EditTask_ChangePriorityFromLowToHigh_SavesHigh()
    {
        await OpenCreateModalAsync();
        await FillAndSaveTaskAsync("Priority Change Task", priority: "1");
        await Page.WaitForSelectorAsync("#tasksBody tr", new() { Timeout = 5_000 });

        // Edit first row
        await Page.Locator("#tasksBody tr").First.Locator("button.btn-outline-primary").ClickAsync();
        await Page.WaitForSelectorAsync("#taskModal.show", new() { Timeout = 5_000 });

        await Page.SelectOptionAsync("#taskPriority", "3");
        await Page.ClickAsync("#saveBtn");
        await Page.WaitForSelectorAsync("#taskModal:not(.show)", new() { Timeout = 8_000 });

        await Expect(Page.Locator("#tasksBody .badge-priority-high").First).ToBeVisibleAsync();
    }

    [Test]
    public async Task EditModal_PopulatesExistingValues()
    {
        await OpenCreateModalAsync();
        await FillAndSaveTaskAsync("Prefill Check", priority: "3");
        await Page.WaitForSelectorAsync("#tasksBody tr", new() { Timeout = 5_000 });

        // Open edit modal
        await Page.Locator("#tasksBody tr").First.Locator("button.btn-outline-primary").ClickAsync();
        await Page.WaitForSelectorAsync("#taskModal.show", new() { Timeout = 5_000 });

        string title    = await Page.Locator("#taskTitle").InputValueAsync();
        string priority = await Page.Locator("#taskPriority").InputValueAsync();

        Assert.That(title,    Does.Contain("Prefill Check"));
        Assert.That(priority, Is.EqualTo("3"));
    }

    // ─── Delete ─────────────────────────────────────────────────────────────────
    [Test]
    public async Task DeleteTask_RemovesTaskFromList()
    {
        await OpenCreateModalAsync();
        await FillAndSaveTaskAsync("Task To Delete");
        await Page.WaitForSelectorAsync("#tasksBody tr", new() { Timeout = 5_000 });

        int before = await Page.Locator("#tasksBody tr").CountAsync();

        // Click delete on first row, confirm
        await Page.Locator("#tasksBody tr").First.Locator("button.btn-outline-danger").ClickAsync();
        await Page.WaitForSelectorAsync("#deleteModal.show", new() { Timeout = 5_000 });

        // Click confirm, wait for DELETE then the reload GET that follows
        await Page.RunAndWaitForResponseAsync(
            async () => await Page.ClickAsync("#confirmDeleteBtn"),
            r => r.Request.Method == "DELETE" && r.Url.Contains("/api/tasks/"),
            new() { Timeout = 8_000 }
        );
        // Give JS a tick to start the follow-up loadTasks() GET, then wait for idle
        await Page.WaitForFunctionAsync("() => document.getElementById('tasksLoading') !== null");
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Poll until the UI reflects the deletion (up to 5 s)
        await Page.WaitForFunctionAsync(
            $"() => document.querySelectorAll('#tasksBody tr').length < {before} || " +
            "!document.getElementById('tasksEmpty').classList.contains('d-none')",
            null, new() { Timeout = 5_000 }
        );

        int after = await Page.Locator("#tasksBody tr").CountAsync();
        bool emptyShown = await Page.Locator("#tasksEmpty").IsVisibleAsync();
        Assert.That(after < before || emptyShown, Is.True, "Task should be removed or empty state should appear");
    }

    [Test]
    public async Task DeleteTask_CancelDoesNotRemoveTask()
    {
        await OpenCreateModalAsync();
        await FillAndSaveTaskAsync("Do Not Delete Me");
        await Page.WaitForSelectorAsync("#tasksBody tr", new() { Timeout = 5_000 });

        int before = await Page.Locator("#tasksBody tr").CountAsync();

        // Open delete modal, then cancel
        await Page.Locator("#tasksBody tr").First.Locator("button.btn-outline-danger").ClickAsync();
        await Page.WaitForSelectorAsync("#deleteModal.show", new() { Timeout = 5_000 });
        await Page.ClickAsync("#deleteModal .btn-secondary");
        await Page.WaitForSelectorAsync("#deleteModal:not(.show)", new() { Timeout = 5_000 });

        int after = await Page.Locator("#tasksBody tr").CountAsync();
        Assert.That(after, Is.EqualTo(before));
    }
}
