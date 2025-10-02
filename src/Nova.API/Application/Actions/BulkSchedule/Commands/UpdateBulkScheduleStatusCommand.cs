using FluentResults;
using FluentValidation;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;
using MediatR;

namespace Nova.API.Application.Actions.BulkSchedule.Commands;

public record UpdateBulkScheduleStatusCommand(string bulkScheduleId, UpdateBulkScheduleStatusRequest request) : MediatR.IRequest<Result<bool>>;

public class UpdateBulkScheduleStatusCommandHandler : MediatR.IRequestHandler<UpdateBulkScheduleStatusCommand, Result<bool>>
{
    private readonly IBulkScheduleService _bulkScheduleService;

    public UpdateBulkScheduleStatusCommandHandler(IBulkScheduleService bulkScheduleService)
    {
        _bulkScheduleService = bulkScheduleService;
    }

    public async Task<Result<bool>> Handle(UpdateBulkScheduleStatusCommand request, CancellationToken cancellationToken)
    {
        return await _bulkScheduleService.UpdateBulkScheduleStatusAsync(request.bulkScheduleId, request.request);
    }
}

public class UpdateBulkScheduleStatusCommandValidator : AbstractValidator<UpdateBulkScheduleStatusCommand>
{
    public UpdateBulkScheduleStatusCommandValidator()
    {
        RuleFor(x => x.bulkScheduleId)
            .NotEmpty()
            .WithMessage("Bulk schedule ID is required");

        RuleFor(x => x.request.Status)
            .NotEmpty()
            .Must(x => new[] { "Draft", "Ready", "Processed", "Completed", "Cancelled" }.Contains(x))
            .WithMessage("Status must be one of: Draft, Ready, Processed, Completed, Cancelled");

        RuleFor(x => x.request.Remarks)
            .MaximumLength(1000)
            .WithMessage("Remarks must not exceed 1000 characters");
    }
}