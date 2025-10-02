using FluentResults;
using MediatR;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;

namespace Nova.API.Application.Actions.BulkSchedule.Queries;

public record GetBulkSchedulesQuery() : MediatR.IRequest<Result<List<BulkScheduleListResponse>>>;

public class GetBulkSchedulesQueryHandler : MediatR.IRequestHandler<GetBulkSchedulesQuery, Result<List<BulkScheduleListResponse>>>
{
    private readonly IBulkScheduleService _bulkScheduleService;

    public GetBulkSchedulesQueryHandler(IBulkScheduleService bulkScheduleService)
    {
        _bulkScheduleService = bulkScheduleService;
    }

    public async Task<Result<List<BulkScheduleListResponse>>> Handle(GetBulkSchedulesQuery request, CancellationToken cancellationToken)
    {
        return await _bulkScheduleService.GetBulkSchedulesForCurrentTenantAsync();
    }
}