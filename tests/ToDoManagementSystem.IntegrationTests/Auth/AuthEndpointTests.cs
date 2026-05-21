namespace ToDoManagementSystem.IntegrationTests.Auth;

public class AuthEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public AuthEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_Returns201AndToken()
    {
        // Arrange
        RegisterRequest request = new()
        {
            FullName = "Integration Test User",
            Email = $"test_{Guid.NewGuid():N}@example.com",
            Password = "Password@123"
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/register", request);
        string json = await response.Content.ReadAsStringAsync();
        ApiResponse<LoginResponse>? result = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(json, _json);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsFailure()
    {
        // Arrange
        string email = $"duplicate_{Guid.NewGuid():N}@example.com";
        RegisterRequest request = new() { FullName = "User", Email = email, Password = "Password@123" };
        await _client.PostAsJsonAsync("/api/auth/register", request);

        // Act — register again with same email
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/register", request);
        string json = await response.Content.ReadAsStringAsync();
        ApiResponse<LoginResponse>? result = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(json, _json);

        // Assert — duplicate email is rejected (ValidationException → 400)
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange — register first, then login
        string email = $"login_{Guid.NewGuid():N}@example.com";
        RegisterRequest registerReq = new() { FullName = "Login User", Email = email, Password = "Password@123" };
        await _client.PostAsJsonAsync("/api/auth/register", registerReq);

        LoginRequest loginReq = new() { Email = email, Password = "Password@123" };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/login", loginReq);
        string json = await response.Content.ReadAsStringAsync();
        ApiResponse<LoginResponse>? result = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(json, _json);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Success.Should().BeTrue();
        result.Data!.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsFailure()
    {
        // Arrange
        string email = $"wrongpw_{Guid.NewGuid():N}@example.com";
        RegisterRequest reg = new() { FullName = "User", Email = email, Password = "Password@123" };
        await _client.PostAsJsonAsync("/api/auth/register", reg);

        LoginRequest loginReq = new() { Email = email, Password = "WrongPassword!" };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/login", loginReq);
        string json = await response.Content.ReadAsStringAsync();
        ApiResponse<LoginResponse>? result = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(json, _json);

        // Assert — UnauthorizedException → 401 from the exception handler
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ReturnsFailure()
    {
        // Arrange
        LoginRequest loginReq = new() { Email = "nobody@nowhere.com", Password = "Any@123" };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/login", loginReq);
        string json = await response.Content.ReadAsStringAsync();
        ApiResponse<LoginResponse>? result = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(json, _json);

        // Assert — UnauthorizedException → 401
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result!.Success.Should().BeFalse();
    }
}
