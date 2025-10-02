using FluentResults;
using MediatR;
using Nova.API.Application.Services.Data;

namespace Nova.API.Application.Actions.BulkSchedule.Queries;

public record ExportBulkScheduleToCsvQuery(string bulkScheduleId) : MediatR.IRequest<Result<byte[]>>;

public class ExportBulkScheduleToCsvQueryHandler : MediatR.IRequestHandler<ExportBulkScheduleToCsvQuery, Result<byte[]>>
{
    private readonly IBulkScheduleService _bulkScheduleService;

    public ExportBulkScheduleToCsvQueryHandler(IBulkScheduleService bulkScheduleService)
    {
        _bulkScheduleService = bulkScheduleService;
    }

    public async Task<Result<byte[]>> Handle(ExportBulkScheduleToCsvQuery request, CancellationToken cancellationToken)
    {
        return await _bulkScheduleService.ExportBulkScheduleToCsvAsync(request.bulkScheduleId);
    }
}