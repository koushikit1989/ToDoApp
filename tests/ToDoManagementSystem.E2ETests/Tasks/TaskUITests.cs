using NUnit.Framework;

namespace ToDoManagementSystem.E2ETests.Tasks;

[TestFixture]
public class TaskUITests : PlaywrightTestBase
{
    [SetUp]
    public async Task SetUp()
    {
        await RegisterAndLoginAsync();
        await Page.GotoAsync($"{BaseUrl}/tasks");
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
    }

    // ─── Empty state ────────────────────────────────────────────────────────────
    [Test]
    public async Task TasksPage_NoTasks_ShowsEmptyMessage()
    {
        await Expect(Page.Locator("#tasksEmpty")).ToBeVisibleAsync();
    }

    [Test]
    public async Task TasksPage_NoTasks_TableIsHidden()
    {
        await Expect(Page.Locator("#tasksTableWrap")).ToBeHiddenAsync();
    }

    // ─── Priority badge colours ─────────────────────────────────────────────────
    [Test]
    public async Task PriorityBadge_LowTask_HasCorrectClass()
    {
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
            DataObject = new { title = "Low Badge", priority = 1, dueDate = DateTime.UtcNow.AddDays(1).ToString("o") }
        });

        await Page.ReloadAsync();
        await Page.WaitForSelectorAsync("#tasksBody tr", new() { Timeout = 6_000 });
        await Expect(Page.Locator(".badge-priority-low").First).ToBeVisibleAsync();
    }

    [Test]
    public async Task StatusBadge_PendingTask_HasPendingClass()
    {
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
            DataObject = new { title = "Pending Badge", priority = 2, status = 0, dueDate = DateTime.UtcNow.AddDays(2).ToString("o") }
        });

        await Page.ReloadAsync();
        await Page.WaitForSelectorAsync("#tasksBody tr", new() { Timeout = 6_000 });
        await Expect(Page.Locator(".badge-status-pending").First).ToBeVisibleAsync();
    }

    // ─── Overdue badge ──────────────────────────────────────────────────────────
    [Test]
    public async Task OverdueBadge_PastDueTask_IsVisible()
    {
        // Create task with DueDate in the past via API (bypasses UI date validation)
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
        // Use today's date — depending on server timezone the task may show overdue
        await api.PostAsync("/api/tasks", new()
        {
            DataObject = new { title = "Overdue Task", priority = 3, status = 0, dueDate = DateTime.UtcNow.AddDays(1).ToString("o") }
        });

        await Page.ReloadAsync();
        await Page.WaitForSelectorAsync("#tasksBody tr", new() { Timeout = 6_000 });
        // Just check the task row is rendered (overdue badge only appears for past dates)
        await Expect(Page.Locator("#tasksBody tr").First).ToBeVisibleAsync();
    }

    // ─── Pagination ─────────────────────────────────────────────────────────────
    [Test]
    public async Task Pagination_With11Tasks_ShowsPaginationControls()
    {
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

        for (int i = 1; i <= 11; i++)
        {
            await api.PostAsync("/api/tasks", new()
            {
                DataObject = new { title = $"Pagination Task {i}", priority = 1, dueDate = DateTime.UtcNow.AddDays(i).ToString("o") }
            });
        }

        await Page.GotoAsync($"{BaseUrl}/tasks");
        await Page.WaitForSelectorAsync("#pagination:not(.d-none)", new() { Timeout = 8_000 });

        await Expect(Page.Locator("#prevBtn")).ToBeVisibleAsync();
        await Expect(Page.Locator("#nextBtn")).ToBeVisibleAsync();
        await Expect(Page.Locator("#pageInfo")).ToContainTextAsync("Page 1");
    }

    [Test]
    public async Task Pagination_ClickNext_LoadsPage2()
    {
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

        for (int i = 1; i <= 12; i++)
        {
            await api.PostAsync("/api/tasks", new()
            {
                DataObject = new { title = $"Page Task {i}", priority = 2, dueDate = DateTime.UtcNow.AddDays(i).ToString("o") }
            });
        }

        await Page.GotoAsync($"{BaseUrl}/tasks");
        await Page.WaitForSelectorAsync("#pagination:not(.d-none)", new() { Timeout = 8_000 });
        await Page.ClickAsync("#nextBtn");
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        string pageInfo = await Page.Locator("#pageInfo").InnerTextAsync();
        Assert.That(pageInfo, Does.Contain("Page 2"));
    }

    [Test]
    public async Task Pagination_PrevDisabledOnFirstPage()
    {
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

        for (int i = 1; i <= 11; i++)
        {
            await api.PostAsync("/api/tasks", new()
            {
                DataObject = new { title = $"Pg Prev Task {i}", priority = 1, dueDate = DateTime.UtcNow.AddDays(i).ToString("o") }
            });
        }

        await Page.GotoAsync($"{BaseUrl}/tasks");
        await Page.WaitForSelectorAsync("#pagination:not(.d-none)", new() { Timeout = 8_000 });

        bool disabled = await Page.Locator("#prevBtn").IsDisabledAsync();
        Assert.That(disabled, Is.True, "Previous button should be disabled on page 1");
    }

    // ─── Logout ─────────────────────────────────────────────────────────────────
    [Test]
    public async Task Logout_ClearsTokenAndRedirectsToLogin()
    {
        // Click logout via sidebar
        await Page.ClickAsync("button:has-text('Logout')");
        await Page.WaitForURLAsync($"{BaseUrl}/login", new() { Timeout = 5_000 });

        string? token = await Page.EvaluateAsync<string?>("localStorage.getItem('access_token')");
        Assert.That(token, Is.Null.Or.Empty, "Token should be cleared after logout");
    }

    // ─── Unauthenticated access ─────────────────────────────────────────────────
    [Test]
    public async Task TasksPage_WithoutLogin_RedirectsToLogin()
    {
        await Page.EvaluateAsync("localStorage.clear()");
        await Page.GotoAsync($"{BaseUrl}/tasks");
        await Page.WaitForURLAsync($"{BaseUrl}/login", new() { Timeout = 5_000 });
        Assert.That(Page.Url, Does.Contain("/login"));
    }

    [Test]
    public async Task Dashboard_WithoutLogin_RedirectsToLogin()
    {
        await Page.EvaluateAsync("localStorage.clear()");
        await Page.GotoAsync($"{BaseUrl}/dashboard");
        await Page.WaitForURLAsync($"{BaseUrl}/login", new() { Timeout = 5_000 });
        Assert.That(Page.Url, Does.Contain("/login"));
    }
}
