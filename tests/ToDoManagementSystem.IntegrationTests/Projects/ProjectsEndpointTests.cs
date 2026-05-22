using ToDoManagementSystem.Application.DTOs.Projects;

namespace ToDoManagementSystem.IntegrationTests.Projects;

public class ProjectsEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly HttpClient _authClient;
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public ProjectsEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _authClient = factory.CreateClient();
    }

    private async Task<string> GetTokenAsync()
    {
        string email = $"projtest_{Guid.NewGuid():N}@example.com";
        RegisterRequest reg = new() { FullName = "Project Tester", Email = email, Password = "Password@123" };
        HttpResponseMessage regRes = await _client.PostAsJsonAsync("/api/auth/register", reg);
        string regJson = await regRes.Content.ReadAsStringAsync();
        ApiResponse<LoginResponse>? result = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(regJson, _json);
        return result!.Data!.AccessToken;
    }

    [Fact]
    public async Task GetProjects_WithoutAuth_Returns401()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/projects");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProject_WithValidData_Returns201WithProject()
    {
        // Arrange
        string token = await GetTokenAsync();
        _authClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        CreateProjectRequest request = new()
        {
            ProjectName = $"Integration Project {Guid.NewGuid():N}",
            ProjectCode = "INT",
            Description = "Created by integration test"
        };

        // Act
        HttpResponseMessage response = await _authClient.PostAsJsonAsync("/api/projects", request);
        string json = await response.Content.ReadAsStringAsync();
        ApiResponse<ProjectResponse>? result = JsonSerializer.Deserialize<ApiResponse<ProjectResponse>>(json, _json);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        result!.Success.Should().BeTrue();
        result.Data!.ProjectName.Should().Be(request.ProjectName);
        result.Data.IsActive.Should().BeTrue();
        result.Data.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAllProjects_AfterCreatingOne_ReturnsProjects()
    {
        // Arrange
        string token = await GetTokenAsync();
        _authClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        CreateProjectRequest createReq = new()
        {
            ProjectName = $"Listed Project {Guid.NewGuid():N}"
        };
        await _authClient.PostAsJsonAsync("/api/projects", createReq);

        // Act
        HttpResponseMessage response = await _authClient.GetAsync("/api/projects");
        string json = await response.Content.ReadAsStringAsync();
        ApiResponse<IEnumerable<ProjectResponse>>? result =
            JsonSerializer.Deserialize<ApiResponse<IEnumerable<ProjectResponse>>>(json, _json);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateProject_WithEmptyName_ReturnsValidationError()
    {
        // Arrange
        string token = await GetTokenAsync();
        _authClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        CreateProjectRequest request = new() { ProjectName = "" };

        // Act
        HttpResponseMessage response = await _authClient.PostAsJsonAsync("/api/projects", request);
        string json = await response.Content.ReadAsStringAsync();
        ApiResponse<ProjectResponse>? result =
            JsonSerializer.Deserialize<ApiResponse<ProjectResponse>>(json, _json);

        // Assert
        result!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteProject_ExistingProject_ReturnsSuccess()
    {
        // Arrange
        string token = await GetTokenAsync();
        _authClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        CreateProjectRequest createReq = new()
        {
            ProjectName = $"Delete Me {Guid.NewGuid():N}"
        };
        HttpResponseMessage createRes = await _authClient.PostAsJsonAsync("/api/projects", createReq);
        string createJson = await createRes.Content.ReadAsStringAsync();
        ApiResponse<ProjectResponse>? created =
            JsonSerializer.Deserialize<ApiResponse<ProjectResponse>>(createJson, _json);
        Guid projectId = created!.Data!.Id;

        // Act
        HttpResponseMessage response = await _authClient.DeleteAsync($"/api/projects/{projectId}");
        string json = await response.Content.ReadAsStringAsync();
        ApiResponse<bool>? result = JsonSerializer.Deserialize<ApiResponse<bool>>(json, _json);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CreateTask_WithProjectId_ReturnsTaskWithProjectName()
    {
        // Arrange
        string token = await GetTokenAsync();
        _authClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create a project first
        CreateProjectRequest projReq = new()
        {
            ProjectName = $"Task Project {Guid.NewGuid():N}"
        };
        HttpResponseMessage projRes = await _authClient.PostAsJsonAsync("/api/projects", projReq);
        string projJson = await projRes.Content.ReadAsStringAsync();
        ApiResponse<ProjectResponse>? project =
            JsonSerializer.Deserialize<ApiResponse<ProjectResponse>>(projJson, _json);
        Guid projectId = project!.Data!.Id;

        // Create a task under that project
        CreateTaskRequest taskReq = new()
        {
            Title = "Task with Project",
            Priority = 2,
            DueDate = DateTime.UtcNow.AddDays(5),
            ProjectId = projectId
        };

        // Act
        HttpResponseMessage response = await _authClient.PostAsJsonAsync("/api/tasks", taskReq);
        string json = await response.Content.ReadAsStringAsync();
        ApiResponse<TaskResponse>? result =
            JsonSerializer.Deserialize<ApiResponse<TaskResponse>>(json, _json);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        result!.Success.Should().BeTrue();
        result.Data!.ProjectId.Should().Be(projectId);
    }
}
