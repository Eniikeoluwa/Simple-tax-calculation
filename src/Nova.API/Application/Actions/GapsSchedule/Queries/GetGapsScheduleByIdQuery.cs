using MediatR;
using FluentResults;
using FluentValidation;
using Nova.Contracts.Models;
using Nova.Domain.Utils;
using Nova.API.Application.Services.Data;

namespace Nova.API.Application.Actions.GapsSchedule.Queries;

public record GetGapsScheduleByIdQuery(string BatchNumber) : IRequest<Result<GapsScheduleResponse>>;

public class GetGapsScheduleByIdHandler : IRequestHandler<GetGapsScheduleByIdQuery, Result<GapsScheduleResponse>>
{
    private readonly IGapsScheduleService _gapsScheduleService;

    public GetGapsScheduleByIdHandler(IGapsScheduleService gapsScheduleService)
    {
        _gapsScheduleService = gapsScheduleService;
    }

    public async Task<Result<GapsScheduleResponse>> Handle(GetGapsScheduleByIdQuery query, CancellationToken cancellationToken)
    {
        return await _gapsScheduleService.GetGapsScheduleByIdAsync(query.BatchNumber);
    }
}