using FluentResults;
using MediatR;
using Nova.API.Application.Services.Data;

namespace Nova.API.Application.Actions.BulkSchedule.Queries;

using Nova.Contracts.Models;

public record ExportBulkScheduleToCsvQuery(string bulkScheduleId) : MediatR.IRequest<Result<BulkScheduleExportResponse>>;

public class ExportBulkScheduleToCsvQueryHandler : MediatR.IRequestHandler<ExportBulkScheduleToCsvQuery, Result<BulkScheduleExportResponse>>
{
    private readonly IBulkScheduleService _bulkScheduleService;

    public ExportBulkScheduleToCsvQueryHandler(IBulkScheduleService bulkScheduleService)
    {
        _bulkScheduleService = bulkScheduleService;
    }

    public async Task<Result<BulkScheduleExportResponse>> Handle(ExportBulkScheduleToCsvQuery request, CancellationToken cancellationToken)
    {
        return await _bulkScheduleService.ExportBulkScheduleToCsvAsync(request.bulkScheduleId);
    }
}