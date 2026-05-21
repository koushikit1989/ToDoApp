using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace ToDoManagementSystem.E2ETests;

/// <summary>Base class for all Playwright UI tests. Runs headless Chromium against the live dev server.</summary>
[Parallelizable(ParallelScope.Self)]
public abstract class PlaywrightTestBase : PageTest
{
    protected const string BaseUrl = "http://localhost:5194";

    // Unique e-mail per test run so DB rows never collide
    protected string UniqueEmail => $"e2e_{TestContext.CurrentContext.Test.ID.GetHashCode():X8}@test.local";
    protected const string TestPassword = "E2eTest@123";
    protected const string TestFullName = "E2E Tester";

    /// <summary>Register a user and return the page, ready for authenticated use.</summary>
    protected async Task RegisterAndLoginAsync(string? email = null, string? password = null)
    {
        string mail = email ?? UniqueEmail;
        string pw   = password ?? TestPassword;

        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Switch to Register tab
        await Page.ClickAsync("#tabRegister");
        await Page.FillAsync("#regName",    TestFullName);
        await Page.FillAsync("#regEmail",   mail);
        await Page.FillAsync("#regPassword", pw);
        await Page.FillAsync("#regConfirm", pw);
        await Page.ClickAsync("#registerBtn");

        // Wait for redirect to dashboard
        await Page.WaitForURLAsync($"{BaseUrl}/dashboard", new() { Timeout = 10_000 });
    }
}
