using MediatR;
using FluentResults;
using FluentValidation;
using Nova.Contracts.Models;
using Nova.Domain.Utils;
using Nova.API.Application.Services.Data;

namespace Nova.API.Application.Actions.GapsSchedule.Commands;

public record ExportGapsScheduleCommand(string BatchNumber) : IRequest<Result<GapsScheduleExportResponse>>;

public class ExportGapsScheduleHandler : IRequestHandler<ExportGapsScheduleCommand, Result<GapsScheduleExportResponse>>
{
    private readonly IGapsScheduleService _gapsScheduleService;

    public ExportGapsScheduleHandler(IGapsScheduleService gapsScheduleService)
    {
        _gapsScheduleService = gapsScheduleService;
    }

    public async Task<Result<GapsScheduleExportResponse>> Handle(ExportGapsScheduleCommand command, CancellationToken cancellationToken)
    {
        return await _gapsScheduleService.ExportToExcelAsync(command.BatchNumber);
    }
}