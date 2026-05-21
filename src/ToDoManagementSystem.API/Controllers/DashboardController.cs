using System.Security.Claims;
using ClosedXML.Excel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoManagementSystem.Application.DTOs.Dashboard;
using ToDoManagementSystem.Application.DTOs.Tasks;
using ToDoManagementSystem.Application.Features.Dashboard.Queries;
using ToDoManagementSystem.Shared.Responses;

namespace ToDoManagementSystem.API.Controllers;

/// <summary>Dashboard and reporting endpoints — all require JWT authentication.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator) => _mediator = mediator;

    /// <summary>Returns aggregated task counts for the current user.</summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummaryResponse>), 200)]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        GetDashboardSummaryQuery query = new() { UserId = GetCurrentUserId() };
        DashboardSummaryResponse result = await _mediator.Send(query, ct);
        return Ok(ApiResponse<DashboardSummaryResponse>.Ok(result));
    }

    /// <summary>Returns detailed report data including recent tasks and priority breakdown.</summary>
    [HttpGet("reports")]
    [ProducesResponseType(typeof(ApiResponse<ReportResponse>), 200)]
    public async Task<IActionResult> GetReports(CancellationToken ct)
    {
        GetReportsQuery query = new() { UserId = GetCurrentUserId() };
        ReportResponse result = await _mediator.Send(query, ct);
        return Ok(ApiResponse<ReportResponse>.Ok(result));
    }

    /// <summary>Exports all tasks for the current user as an Excel file.</summary>
    [HttpGet("export")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> Export([FromQuery] string format = "excel", CancellationToken ct = default)
    {
        ExportTasksQuery query = new() { UserId = GetCurrentUserId() };
        IEnumerable<TaskResponse> allTasks = await _mediator.Send(query, ct);

        using XLWorkbook workbook = new();
        IXLWorksheet sheet = workbook.Worksheets.Add("Tasks Report");

        // Header row
        string[] headers = { "Title", "Priority", "Status", "Due Date", "Created Date", "Overdue" };
        for (int i = 0; i < headers.Length; i++)
        {
            IXLCell cell = sheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.SteelBlue;
            cell.Style.Font.FontColor = XLColor.White;
        }

        // Data rows
        int row = 2;
        foreach (TaskResponse task in allTasks)
        {
            sheet.Cell(row, 1).Value = task.Title;
            sheet.Cell(row, 2).Value = task.Priority;
            sheet.Cell(row, 3).Value = task.Status;
            sheet.Cell(row, 4).Value = task.DueDate.ToString("yyyy-MM-dd");
            sheet.Cell(row, 5).Value = task.CreatedDate.ToString("yyyy-MM-dd");
            sheet.Cell(row, 6).Value = task.IsOverdue ? "Yes" : "No";
            row++;
        }

        sheet.Columns().AdjustToContents();

        using MemoryStream stream = new();
        workbook.SaveAs(stream);

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"tasks-report-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
