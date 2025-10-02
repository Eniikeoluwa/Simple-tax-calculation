using FluentResults;
using FluentValidation;
using Nova.API.Application.Services.Data;
using MediatR;

namespace Nova.API.Application.Actions.BulkSchedule.Commands;

public record DeleteBulkScheduleCommand(string bulkScheduleId) : MediatR.IRequest<Result<bool>>;

public class DeleteBulkScheduleCommandHandler : MediatR.IRequestHandler<DeleteBulkScheduleCommand, Result<bool>>
{
    private readonly IBulkScheduleService _bulkScheduleService;

    public DeleteBulkScheduleCommandHandler(IBulkScheduleService bulkScheduleService)
    {
        _bulkScheduleService = bulkScheduleService;
    }

    public async Task<Result<bool>> Handle(DeleteBulkScheduleCommand request, CancellationToken cancellationToken)
    {
        return await _bulkScheduleService.DeleteBulkScheduleAsync(request.bulkScheduleId);
    }
}

public class DeleteBulkScheduleCommandValidator : AbstractValidator<DeleteBulkScheduleCommand>
{
    public DeleteBulkScheduleCommandValidator()
    {
        RuleFor(x => x.bulkScheduleId)
            .NotEmpty()
            .WithMessage("Bulk schedule ID is required");
    }
}