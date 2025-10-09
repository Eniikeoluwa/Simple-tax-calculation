using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Nova.API.Application.Actions.GapsSchedule.Commands;
using Nova.API.Application.Actions.GapsSchedule.Queries;
using Nova.Contracts.Models;

namespace Nova.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GapsScheduleController : BaseController
{
    public GapsScheduleController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost("generate")]
    public async Task<ActionResult<List<GapsScheduleResponse>>> GenerateGapsSchedule(
        [FromBody] GenerateGapsScheduleRequest request)
    {
        var command = new GenerateGapsScheduleCommand(request);
        return await SendCommand<GenerateGapsScheduleCommand, List<GapsScheduleResponse>>(command);
    }

    [HttpGet("list")]
    public async Task<ActionResult<List<GapsScheduleListResponse>>> GetGapsSchedules()
    {
        var query = new GetGapsSchedulesQuery();
        return await SendQuery<GetGapsSchedulesQuery, List<GapsScheduleListResponse>>(query);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GapsScheduleResponse>> GetGapsScheduleById(string id)
    {
        var query = new GetGapsScheduleByIdQuery(id);
        return await SendQuery<GetGapsScheduleByIdQuery, GapsScheduleResponse>(query);
    }

    [HttpGet("export/{batchNumber}")]
    public async Task<ActionResult> ExportToExcel(string batchNumber)
    {
        var command = new ExportGapsScheduleCommand(batchNumber);
        var result = await SendCommand<ExportGapsScheduleCommand, GapsScheduleExportResponse>(command);

        return File(
            result.Value.FileContent,
            result.Value.ContentType,
            result.Value.FileName
        );
    }
}
