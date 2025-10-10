using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nova.API.Application.Actions.BulkSchedule.Commands;
using Nova.API.Application.Actions.BulkSchedule.Queries;
using Nova.API.Controllers;
using Nova.Contracts.Models;
using MediatR;

namespace Nova.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BulkScheduleController : BaseController
{
    public BulkScheduleController(IMediator mediator) : base(mediator)
    {
    }
    [HttpPost("create")]
    public async Task<ActionResult<BulkScheduleResponse>> GenerateBulkSchedule(
        [FromBody] CreateBulkScheduleRequest request)
    {
        var command = new GenerateBulkScheduleCommand(request);
        return await SendCommand<GenerateBulkScheduleCommand, BulkScheduleResponse>(command);
    }

    [HttpGet("list")]
    public async Task<ActionResult<List<BulkScheduleListResponse>>> GetBulkSchedules()
    {
        var query = new GetBulkSchedulesQuery();
        return await SendQuery<GetBulkSchedulesQuery, List<BulkScheduleListResponse>>(query);
    }

    [HttpGet("{bulkScheduleId}")]
    public async Task<ActionResult<BulkScheduleResponse>> GetBulkScheduleById(string bulkScheduleId)
    {
        var query = new GetBulkScheduleByIdQuery(bulkScheduleId);
        return await SendQuery<GetBulkScheduleByIdQuery, BulkScheduleResponse>(query);
    }

    [HttpPatch("{bulkScheduleId}/status")]
    public async Task<ActionResult<bool>> UpdateBulkScheduleStatus(
        string bulkScheduleId,
        [FromBody] UpdateBulkScheduleStatusRequest request)
    {
        var command = new UpdateBulkScheduleStatusCommand(bulkScheduleId, request);
        return await SendCommand<UpdateBulkScheduleStatusCommand, bool>(command);
    }

    [HttpPatch("{bulkScheduleId}/approve")]
    public async Task<ActionResult<bool>> ApproveBulkSchedule(
        string bulkScheduleId,
        [FromBody] ApproveBulkScheduleRequest request)
    {
        var command = new ApproveBulkScheduleCommand(bulkScheduleId, request);
        return await SendCommand<ApproveBulkScheduleCommand, bool>(command);
    }

    [HttpDelete("{bulkScheduleId}")]
    public async Task<ActionResult<bool>> DeleteBulkSchedule(string bulkScheduleId)
    {
        var command = new DeleteBulkScheduleCommand(bulkScheduleId);
        return await SendCommand<DeleteBulkScheduleCommand, bool>(command);
    }

    [HttpGet("{bulkScheduleId}/export/csv")]
    public async Task<IActionResult> ExportBulkScheduleToCsv(string bulkScheduleId)
    {
        var query = new ExportBulkScheduleToCsvQuery(bulkScheduleId);
        var result = await SendQuery<ExportBulkScheduleToCsvQuery, BulkScheduleExportResponse>(query);

        if (result.Result is OkObjectResult okResult && okResult.Value is BulkScheduleExportResponse exportData)
        {
            return File(
                exportData.FileContent,
                exportData.ContentType,
                exportData.FileName
            );
        }

        return result.Result ?? BadRequest("Failed to export bulk schedule");
    }
}