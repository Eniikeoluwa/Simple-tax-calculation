using MediatR;
using FluentResults;
using FluentValidation;
using Nova.Contracts.Models;
using Nova.Domain.Utils;
using Nova.API.Application.Services.Data;

namespace Nova.API.Application.Actions.BulkSchedule.Commands;

public record ExportBulkScheduleCommand(string BulkScheduleId) : IRequest<Result<BulkScheduleExportResponse>>;

public class ExportBulkScheduleHandler : IRequestHandler<ExportBulkScheduleCommand, Result<BulkScheduleExportResponse>>
{
    private readonly IBulkScheduleService _bulkScheduleService;

    public ExportBulkScheduleHandler(IBulkScheduleService bulkScheduleService)
    {
        _bulkScheduleService = bulkScheduleService;
    }

    public async Task<Result<BulkScheduleExportResponse>> Handle(ExportBulkScheduleCommand command, CancellationToken cancellationToken)
    {
        return await _bulkScheduleService.ExportBulkScheduleToCsvAsync(command.BulkScheduleId);
    }
}