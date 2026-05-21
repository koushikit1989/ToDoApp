namespace ToDoManagementSystem.IntegrationTests.Tasks;

public class TasksEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly HttpClient _authClient;
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public TasksEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _authClient = factory.CreateClient();
    }

    private async Task<string> GetTokenAsync()
    {
        string email = $"tasktest_{Guid.NewGuid():N}@example.com";
        RegisterRequest reg = new() { FullName = "Task Tester", Email = email, Password = "Password@123" };
        HttpResponseMessage regRes = await _client.PostAsJsonAsync("/api/auth/register", reg);
        string regJson = await regRes.Content.ReadAsStringAsync();
        ApiResponse<LoginResponse>? regResult = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(regJson, _json);
        return regResult!.Data!.AccessToken;
    }

    [Fact]
    public async Task GetTasks_WithoutAuth_Returns401()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/tasks");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTask_WithValidData_Returns201WithTask()
    {
        // Arrange
        string token = await GetTokenAsync();
        _authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        CreateTaskRequest request = new()
        {
            Title = "Integration Test Task",
            Description = "Created by integration test",
            Priority = 2,
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        // Act
        HttpResponseMessage response = await _authClient.PostAsJsonAsync("/api/tasks", request);
        string json = await response.Content.ReadAsStringAsync();
        ApiResponse<TaskResponse>? result = JsonSerializer.Deserialize<ApiResponse<TaskResponse>>(json, _json);

        // Assert — Create returns 201
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result!.Success.Should().BeTrue();
        result.Data!.Title.Should().Be(request.Title);
        result.Data.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAllTasks_AfterCreatingOne_ReturnsPagedResults()
    {
        // Arrange
        string token = await GetTokenAsync();
        _authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        CreateTaskRequest createReq = new()
        {
            Title = "My Listed Task",
            Priority = 1,
            DueDate = DateTime.UtcNow.AddDays(3)
        };
        await _authClient.PostAsJsonAsync("/api/tasks", createReq);

        // Act
        HttpResponseMessage response = await _authClient.GetAsync("/api/tasks");
        string json = await response.Content.ReadAsStringAsync();
        ApiResponse<PagedResponse<TaskResponse>>? result =
            JsonSerializer.Deserialize<ApiResponse<PagedResponse<TaskResponse>>>(json, _json);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Success.Should().BeTrue();
        result.Data!.Items.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateTask_WithEmptyTitle_ReturnsValidationError()
    {
        // Arrange
        string token = await GetTokenAsync();
        _authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        CreateTaskRequest request = new()
        {
            Title = "",
            Priority = 2,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        HttpResponseMessage response = await _authClient.PostAsJsonAsync("/api/tasks", request);
        string json = await response.Content.ReadAsStringAsync();
        ApiResponse<TaskResponse>? result = JsonSerializer.Deserialize<ApiResponse<TaskResponse>>(json, _json);

        // Assert — ValidationBehavior catches empty title via CreateTaskCommandValidator
        result!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteTask_ExistingTask_ReturnsSuccess()
    {
        // Arrange
        string token = await GetTokenAsync();
        _authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        CreateTaskRequest createReq = new()
        {
            Title = "Task To Delete",
            Priority = 1,
            DueDate = DateTime.UtcNow.AddDays(5)
        };
        HttpResponseMessage createRes = await _authClient.PostAsJsonAsync("/api/tasks", createReq);
        string createJson = await createRes.Content.ReadAsStringAsync();
        ApiResponse<TaskResponse>? created = JsonSerializer.Deserialize<ApiResponse<TaskResponse>>(createJson, _json);
        Guid taskId = created!.Data!.Id;

        // Act
        HttpResponseMessage response = await _authClient.DeleteAsync($"/api/tasks/{taskId}");
        string json = await response.Content.ReadAsStringAsync();
        ApiResponse<bool>? result = JsonSerializer.Deserialize<ApiResponse<bool>>(json, _json);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
    }
}
