using MediatR;
using FluentResults;
using FluentValidation;
using Nova.Contracts.Models;
using Nova.Domain.Utils;
using Nova.API.Application.Services.Data;

namespace Nova.API.Application.Actions.GapsSchedule.Commands;

public record GenerateGapsScheduleCommand(GenerateGapsScheduleRequest Request) : IRequest<Result<GapsScheduleResponse>>;

public class GenerateGapsScheduleHandler : IRequestHandler<GenerateGapsScheduleCommand, Result<GapsScheduleResponse>>
{
    private readonly IGapsScheduleService _gapsScheduleService;

    public GenerateGapsScheduleHandler(IGapsScheduleService gapsScheduleService)
    {
        _gapsScheduleService = gapsScheduleService;
    }

    public async Task<Result<GapsScheduleResponse>> Handle(GenerateGapsScheduleCommand command, CancellationToken cancellationToken)
    {
        return await _gapsScheduleService.GenerateGapsScheduleAsync(command.Request);
    }
}