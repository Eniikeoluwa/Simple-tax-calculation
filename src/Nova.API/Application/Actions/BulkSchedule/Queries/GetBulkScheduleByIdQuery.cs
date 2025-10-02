using FluentResults;
using MediatR;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;

namespace Nova.API.Application.Actions.BulkSchedule.Queries;

public record GetBulkScheduleByIdQuery(string bulkScheduleId) : MediatR.IRequest<Result<BulkScheduleResponse>>;

public class GetBulkScheduleByIdQueryHandler : MediatR.IRequestHandler<GetBulkScheduleByIdQuery, Result<BulkScheduleResponse>>
{
    private readonly IBulkScheduleService _bulkScheduleService;

    public GetBulkScheduleByIdQueryHandler(IBulkScheduleService bulkScheduleService)
    {
        _bulkScheduleService = bulkScheduleService;
    }

    public async Task<Result<BulkScheduleResponse>> Handle(GetBulkScheduleByIdQuery request, CancellationToken cancellationToken)
    {
        return await _bulkScheduleService.GetBulkScheduleByIdAsync(request.bulkScheduleId);
    }
}