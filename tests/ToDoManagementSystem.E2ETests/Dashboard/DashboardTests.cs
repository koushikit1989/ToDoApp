using NUnit.Framework;

namespace ToDoManagementSystem.E2ETests.Dashboard;

[TestFixture]
public class DashboardTests : PlaywrightTestBase
{
    [SetUp]
    public async Task SetUp() => await RegisterAndLoginAsync();

    // ── Page structure ──────────────────────────────────────────────────────────
    [Test]
    public async Task Dashboard_Loads_ShowsStatCards()
    {
        await Expect(Page.Locator("#statTotal")).ToBeVisibleAsync();
        await Expect(Page.Locator("#statCompleted")).ToBeVisibleAsync();
        await Expect(Page.Locator("#statPending")).ToBeVisibleAsync();
        await Expect(Page.Locator("#statOverdue")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Dashboard_Loads_ShowsCompletionProgressBar()
    {
        // #completionBar is a 0-width div inside .progress; check the container instead
        await Expect(Page.Locator(".progress")).ToBeVisibleAsync();
        await Expect(Page.Locator("#completionPct")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Dashboard_Loads_ShowsRecentTasksSection()
    {
        // Wait for loading spinner to become hidden, then check table or empty state
        await Page.Locator("#recentTasksLoading").WaitForAsync(new() { State = Microsoft.Playwright.WaitForSelectorState.Hidden, Timeout = 8_000 });
        bool tableVisible = await Page.Locator("#recentTable").IsVisibleAsync();
        bool emptyVisible = await Page.Locator("#recentEmpty").IsVisibleAsync();
        Assert.That(tableVisible || emptyVisible, Is.True, "Either recent tasks table or empty state must be visible");
    }

    // ── Empty state ─────────────────────────────────────────────────────────────
    [Test]
    public async Task Dashboard_NoTasks_ShowsZeroStats()
    {
        await Page.WaitForSelectorAsync("#statTotal:not(:text('—'))", new() { Timeout = 8_000 });
        string total = await Page.Locator("#statTotal").InnerTextAsync();
        Assert.That(total.Trim(), Is.EqualTo("0"), "New user should have 0 total tasks");
    }

    [Test]
    public async Task Dashboard_NoTasks_CompletionRateIsZero()
    {
        await Page.WaitForSelectorAsync("#completionPct:not(:text('—'))", new() { Timeout = 8_000 });
        string pct = await Page.Locator("#completionPct").InnerTextAsync();
        Assert.That(pct.Trim(), Is.EqualTo("0%"));
    }

    [Test]
    public async Task Dashboard_NoTasks_ShowsEmptyRecentTasksHint()
    {
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
        await Expect(Page.Locator("#recentEmpty")).ToBeVisibleAsync();
    }

    // ── Stats update after task creation ────────────────────────────────────────
    [Test]
    public async Task Dashboard_AfterCreatingTask_StatTotalIncrements()
    {
        // Create a task via API then reload dashboard
        string token = await Page.EvaluateAsync<string>("localStorage.getItem('access_token')");
        var api = await Playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = BaseUrl,
            ExtraHTTPHeaders = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {token}",
                ["Content-Type"] = "application/json"
            }
        });

        await api.PostAsync("/api/tasks", new()
        {
            DataObject = new
            {
                title    = "Dashboard Test Task",
                priority = 2,
                dueDate  = DateTime.UtcNow.AddDays(3).ToString("o")
            }
        });

        // Reload dashboard and check stats
        await Page.GotoAsync($"{BaseUrl}/dashboard");
        await Page.WaitForSelectorAsync("#statTotal:not(:text('—'))", new() { Timeout = 8_000 });
        string total = await Page.Locator("#statTotal").InnerTextAsync();
        Assert.That(int.Parse(total.Trim()), Is.GreaterThan(0));
    }

    // ── Navigation link ─────────────────────────────────────────────────────────
    [Test]
    public async Task Dashboard_NewTaskButton_NavigatesToTasksPage()
    {
        await Page.ClickAsync("a:has-text('New Task')");
        await Page.WaitForURLAsync($"{BaseUrl}/tasks", new() { Timeout = 5_000 });
        Assert.That(Page.Url, Does.Contain("/tasks"));
    }
}
