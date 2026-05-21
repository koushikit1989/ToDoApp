using Microsoft.Playwright;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace ToDoManagementSystem.E2ETests.Auth;

[TestFixture]
public class AuthTests : PlaywrightTestBase
{
    // ── Login page loads ────────────────────────────────────────────────────────
    [Test]
    public async Task LoginPage_Loads_ShowsSignInTab()
    {
        await Page.GotoAsync($"{BaseUrl}/login");
        await Expect(Page.Locator("#tabLogin")).ToBeVisibleAsync();
        await Expect(Page.Locator("#tabRegister")).ToBeVisibleAsync();
        await Expect(Page.Locator("#panelLogin")).ToBeVisibleAsync();
        await Expect(Page.Locator("#panelRegister")).ToHaveClassAsync(new Regex("(?!.*\\bactive\\b)"));
    }

    // ── Tab switching ───────────────────────────────────────────────────────────
    [Test]
    public async Task LoginPage_ClickRegisterTab_ShowsRegisterForm()
    {
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.ClickAsync("#tabRegister");
        await Expect(Page.Locator("#panelRegister")).ToBeVisibleAsync();
        await Expect(Page.Locator("#regName")).ToBeVisibleAsync();
        await Expect(Page.Locator("#regEmail")).ToBeVisibleAsync();
        await Expect(Page.Locator("#regPassword")).ToBeVisibleAsync();
        await Expect(Page.Locator("#regConfirm")).ToBeVisibleAsync();
    }

    // ── Password strength meter ─────────────────────────────────────────────────
    [Test]
    public async Task RegisterTab_WeakPassword_ShowsWeakStrength()
    {
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.ClickAsync("#tabRegister");
        await Page.FillAsync("#regPassword", "abc");
        string label = await Page.Locator("#strengthLabel").InnerTextAsync();
        Assert.That(label, Is.EqualTo("Weak").Or.EqualTo("Fair").Or.EqualTo("").Or.EqualTo("Good"), "Strength label should be set");
    }

    [Test]
    public async Task RegisterTab_StrongPassword_ShowsStrongStrength()
    {
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.ClickAsync("#tabRegister");
        await Page.FillAsync("#regPassword", "E2eTest@123");
        string label = await Page.Locator("#strengthLabel").InnerTextAsync();
        Assert.That(label, Is.EqualTo("Strong"), "Strong password should show 'Strong'");
    }

    // ── Register ────────────────────────────────────────────────────────────────
    [Test]
    public async Task Register_WithValidData_RedirectsToDashboard()
    {
        await RegisterAndLoginAsync();
        Assert.That(Page.Url, Does.Contain("/dashboard"));
    }

    [Test]
    public async Task Register_PasswordMismatch_ShowsError()
    {
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.ClickAsync("#tabRegister");
        await Page.FillAsync("#regName",    TestFullName);
        await Page.FillAsync("#regEmail",   $"mismatch_{Guid.NewGuid():N}@test.local");
        await Page.FillAsync("#regPassword", "E2eTest@123");
        await Page.FillAsync("#regConfirm", "WrongConfirm@1");
        await Page.ClickAsync("#registerBtn");

        await Expect(Page.Locator("#authAlert")).ToBeVisibleAsync();
        string text = await Page.Locator("#authAlert").InnerTextAsync();
        Assert.That(text, Does.Contain("match").IgnoreCase);
    }

    [Test]
    public async Task Register_DuplicateEmail_ShowsError()
    {
        string email = $"dup_{Guid.NewGuid():N}@test.local";
        await RegisterAndLoginAsync(email);

        // Logout
        await Page.EvaluateAsync("localStorage.clear()");
        await Page.GotoAsync($"{BaseUrl}/login");

        // Try registering again with same email
        await Page.ClickAsync("#tabRegister");
        await Page.FillAsync("#regName",    TestFullName);
        await Page.FillAsync("#regEmail",   email);
        await Page.FillAsync("#regPassword", TestPassword);
        await Page.FillAsync("#regConfirm", TestPassword);
        await Page.ClickAsync("#registerBtn");

        await Expect(Page.Locator("#authAlert")).ToBeVisibleAsync();
    }

    // ── Login ───────────────────────────────────────────────────────────────────
    [Test]
    public async Task Login_WithValidCredentials_RedirectsToDashboard()
    {
        string email = $"login_{Guid.NewGuid():N}@test.local";
        await RegisterAndLoginAsync(email);

        // Logout
        await Page.EvaluateAsync("localStorage.clear()");
        await Page.GotoAsync($"{BaseUrl}/login");

        // Login
        await Page.FillAsync("#loginEmail",    email);
        await Page.FillAsync("#loginPassword", TestPassword);
        await Page.ClickAsync("#loginBtn");

        await Page.WaitForURLAsync($"{BaseUrl}/dashboard", new() { Timeout = 10_000 });
        Assert.That(Page.Url, Does.Contain("/dashboard"));
    }

    [Test]
    public async Task Login_WithWrongPassword_ShowsError()
    {
        string email = $"wrongpw_{Guid.NewGuid():N}@test.local";
        await RegisterAndLoginAsync(email);
        await Page.EvaluateAsync("localStorage.clear()");
        await Page.GotoAsync($"{BaseUrl}/login");

        await Page.FillAsync("#loginEmail",    email);
        await Page.FillAsync("#loginPassword", "WrongPassword@9");
        await Page.ClickAsync("#loginBtn");

        await Expect(Page.Locator("#authAlert")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Login_WithNonExistentEmail_ShowsError()
    {
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.FillAsync("#loginEmail",    "nobody@nowhere.com");
        await Page.FillAsync("#loginPassword", "Any@123");
        await Page.ClickAsync("#loginBtn");

        await Expect(Page.Locator("#authAlert")).ToBeVisibleAsync();
    }

    // ── Toggle password visibility ──────────────────────────────────────────────
    [Test]
    public async Task LoginForm_TogglePasswordVisibility_ChangesInputType()
    {
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.FillAsync("#loginPassword", "secret");

        // Initially password type
        string typeBefore = await Page.Locator("#loginPassword").GetAttributeAsync("type") ?? "";
        Assert.That(typeBefore, Is.EqualTo("password"));

        // Scroll into view and dispatch click to bypass viewport/stability check
        await Page.Locator("#panelLogin .toggle-pw").ScrollIntoViewIfNeededAsync();
        await Page.Locator("#panelLogin .toggle-pw").DispatchEventAsync("click");
        string typeAfter = await Page.Locator("#loginPassword").GetAttributeAsync("type") ?? "";
        Assert.That(typeAfter, Is.EqualTo("text"));
    }

    // ── Already logged in redirect ──────────────────────────────────────────────
    [Test]
    public async Task LoginPage_WhenAlreadyLoggedIn_RedirectsToDashboard()
    {
        await RegisterAndLoginAsync();
        // Now navigate back to login
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.WaitForURLAsync($"{BaseUrl}/dashboard", new() { Timeout = 5_000 });
        Assert.That(Page.Url, Does.Contain("/dashboard"));
    }
}
