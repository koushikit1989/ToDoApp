using NUnit.Framework;

namespace ToDoManagementSystem.E2ETests.Tasks;

[TestFixture]
public class TaskFilterTests : PlaywrightTestBase
{
    [SetUp]
    public async Task SetUp()
    {
        await RegisterAndLoginAsync();
        await Page.GotoAsync($"{BaseUrl}/tasks");
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Seed tasks with known attributes via API
        string token = await Page.EvaluateAsync<string>("localStorage.getItem('access_token')");
        var api = await Playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = BaseUrl,
            ExtraHTTPHeaders = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {token}",
                ["Content-Type"]  = "application/json"
            }
        });

        await api.PostAsync("/api/tasks", new()
        {
            DataObject = new { title = "Alpha Low Task",    priority = 1, dueDate = DateTime.UtcNow.AddDays(2).ToString("o") }
        });
        await api.PostAsync("/api/tasks", new()
        {
            DataObject = new { title = "Beta High Task",    priority = 3, dueDate = DateTime.UtcNow.AddDays(4).ToString("o") }
        });

        // Create Gamma Medium Task then PATCH it to InProgress (CreateTask always sets Pending)
        Microsoft.Playwright.IAPIResponse gammaResp = await api.PostAsync("/api/tasks", new()
        {
            DataObject = new { title = "Gamma Medium Task", priority = 2, dueDate = DateTime.UtcNow.AddDays(6).ToString("o") }
        });
        System.Text.Json.JsonDocument gammaJson = System.Text.Json.JsonDocument.Parse(await gammaResp.TextAsync());
        string gammaId = gammaJson.RootElement.GetProperty("data").GetProperty("id").GetString()!;
        await api.PatchAsync($"/api/tasks/{gammaId}/status", new() { DataObject = 1 });

        // Reload to show seeded data
        await Page.GotoAsync($"{BaseUrl}/tasks");
        await Page.WaitForSelectorAsync("#tasksBody tr", new() { Timeout = 8_000 });
    }

    // ─── Priority filter ────────────────────────────────────────────────────────
    private async Task WaitForTasksToLoad()
    {
        // Wait for loading spinner to disappear
        await Page.Locator("#tasksLoading").WaitForAsync(new() { State = Microsoft.Playwright.WaitForSelectorState.Hidden, Timeout = 8_000 });
    }

    [Test]
    public async Task FilterByPriority_Low_ShowsOnlyLowTasks()
    {
        await Page.SelectOptionAsync("#filterPriority", "1");
        await WaitForTasksToLoad();

        var rows = Page.Locator("#tasksBody tr");
        int count = await rows.CountAsync();
        Assert.That(count, Is.GreaterThan(0), "Should have at least one Low task");

        // Every badge should be Low
        var badges = Page.Locator("#tasksBody .badge-priority-medium, #tasksBody .badge-priority-high");
        Assert.That(await badges.CountAsync(), Is.EqualTo(0), "No Medium or High badges should appear");
    }

    [Test]
    public async Task FilterByPriority_High_ShowsOnlyHighTasks()
    {
        await Page.SelectOptionAsync("#filterPriority", "3");
        await WaitForTasksToLoad();

        var lowMedBadges = Page.Locator("#tasksBody .badge-priority-low, #tasksBody .badge-priority-medium");
        Assert.That(await lowMedBadges.CountAsync(), Is.EqualTo(0));
    }

    // ─── Status filter ──────────────────────────────────────────────────────────
    [Test]
    public async Task FilterByStatus_Pending_ShowsOnlyPendingTasks()
    {
        await Page.SelectOptionAsync("#filterStatus", "0");
        await WaitForTasksToLoad();

        var inProgressBadges = Page.Locator("#tasksBody .badge-status-inprogress, #tasksBody .badge-status-completed");
        Assert.That(await inProgressBadges.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task FilterByStatus_InProgress_ShowsOnlyInProgressTasks()
    {
        await Page.SelectOptionAsync("#filterStatus", "1");
        await WaitForTasksToLoad();

        int count = await Page.Locator("#tasksBody tr").CountAsync();
        Assert.That(count, Is.GreaterThan(0), "Gamma Medium Task has InProgress status");
    }

    // ─── Search ─────────────────────────────────────────────────────────────────
    [Test]
    public async Task Search_ByPartialTitle_FiltersResults()
    {
        await Page.FillAsync("#filterSearch", "Alpha");
        await Page.WaitForTimeoutAsync(600); // debounce

        await Expect(Page.Locator("#tasksBody")).ToContainTextAsync("Alpha Low Task");
        var allRows = await Page.Locator("#tasksBody tr").CountAsync();
        Assert.That(allRows, Is.EqualTo(1), "Only Alpha task should match");
    }

    [Test]
    public async Task Search_NoMatch_ShowsEmptyState()
    {
        await Page.FillAsync("#filterSearch", "XYZXYZXYZ_NOMATCH");
        await Page.WaitForTimeoutAsync(600);

        await Expect(Page.Locator("#tasksEmpty")).ToBeVisibleAsync();
    }

    // ─── Clear filters ──────────────────────────────────────────────────────────
    [Test]
    public async Task ClearFilters_ResetsAllDropdownsAndSearch()
    {
        await Page.SelectOptionAsync("#filterPriority", "1");
        await Page.FillAsync("#filterSearch", "Alpha");
        await Page.WaitForTimeoutAsync(600);

        await Page.ClickAsync("button:has-text('Clear')");
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        string priority = await Page.Locator("#filterPriority").InputValueAsync();
        string search   = await Page.Locator("#filterSearch").InputValueAsync();
        Assert.That(priority, Is.EqualTo(""), "Priority filter should be cleared");
        Assert.That(search,   Is.EqualTo(""), "Search should be cleared");
    }

    [Test]
    public async Task ClearFilters_ShowsAllTasksAgain()
    {
        await Page.SelectOptionAsync("#filterPriority", "1");
        await Page.WaitForTimeoutAsync(400);

        int filtered = await Page.Locator("#tasksBody tr").CountAsync();

        await Page.ClickAsync("button:has-text('Clear')");
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
        await Page.WaitForTimeoutAsync(400);

        int all = await Page.Locator("#tasksBody tr").CountAsync();
        Assert.That(all, Is.GreaterThanOrEqualTo(filtered));
    }
}
