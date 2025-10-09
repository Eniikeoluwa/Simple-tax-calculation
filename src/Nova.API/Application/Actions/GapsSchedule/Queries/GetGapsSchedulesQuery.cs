using MediatR;
using Nova.Contracts.Models;
using FluentResults;
using FluentValidation;
using Nova.Domain.Utils;
using Nova.API.Application.Services.Data;

namespace Nova.API.Application.Actions.GapsSchedule.Queries;

public record GetGapsSchedulesQuery : IRequest<Result<List<GapsScheduleListResponse>>>;

public class GetGapsSchedulesHandler : IRequestHandler<GetGapsSchedulesQuery, Result<List<GapsScheduleListResponse>>>
{
    private readonly IGapsScheduleService _gapsScheduleService;

    public GetGapsSchedulesHandler(IGapsScheduleService gapsScheduleService)
    {
        _gapsScheduleService = gapsScheduleService;
    }

    public async Task<Result<List<GapsScheduleListResponse>>> Handle(GetGapsSchedulesQuery query, CancellationToken cancellationToken)
    {
        return await _gapsScheduleService.GetGapsSchedulesForCurrentTenantAsync();
    }
}