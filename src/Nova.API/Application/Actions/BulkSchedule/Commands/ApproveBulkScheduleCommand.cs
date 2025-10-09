using FluentResults;
using FluentValidation;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;
using MediatR;

namespace Nova.API.Application.Actions.BulkSchedule.Commands;

public record ApproveBulkScheduleCommand(string BulkScheduleId, ApproveBulkScheduleRequest request) : MediatR.IRequest<Result<bool>>;

public class ApproveBulkScheduleCommandHandler : MediatR.IRequestHandler<ApproveBulkScheduleCommand, Result<bool>>
{
    private readonly IBulkScheduleService _bulkScheduleService;

    public ApproveBulkScheduleCommandHandler(IBulkScheduleService bulkScheduleService)
    {
        _bulkScheduleService = bulkScheduleService;
    }

    public async Task<Result<bool>> Handle(ApproveBulkScheduleCommand command, CancellationToken cancellationToken)
    {
        return await _bulkScheduleService.ApproveBulkScheduleAsync(command.BulkScheduleId, command.request);
    }
}

public class ApproveBulkScheduleCommandValidator : AbstractValidator<ApproveBulkScheduleCommand>
{
    public ApproveBulkScheduleCommandValidator()
    {
        RuleFor(x => x.BulkScheduleId)
            .NotEmpty()
            .WithMessage("BulkSchedule ID is required");

        RuleFor(x => x.request.Remarks)
            .MaximumLength(1000)
            .WithMessage("Remarks must not exceed 1000 characters");
    }
}