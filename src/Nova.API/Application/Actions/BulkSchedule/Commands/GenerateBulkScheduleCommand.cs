using FluentResults;
using FluentValidation;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;
using MediatR;

namespace Nova.API.Application.Actions.BulkSchedule.Commands;

public record GenerateBulkScheduleCommand(CreateBulkScheduleRequest request) : MediatR.IRequest<Result<BulkScheduleResponse>>;

public class GenerateBulkScheduleCommandHandler : MediatR.IRequestHandler<GenerateBulkScheduleCommand, Result<BulkScheduleResponse>>
{
    private readonly IBulkScheduleService _bulkScheduleService;

    public GenerateBulkScheduleCommandHandler(IBulkScheduleService bulkScheduleService)
    {
        _bulkScheduleService = bulkScheduleService;
    }

    public async Task<Result<BulkScheduleResponse>> Handle(GenerateBulkScheduleCommand request, CancellationToken cancellationToken)
    {
        return await _bulkScheduleService.GenerateBulkScheduleAsync(request.request);
    }
}

public class GenerateBulkScheduleCommandValidator : AbstractValidator<GenerateBulkScheduleCommand>
{
    public GenerateBulkScheduleCommandValidator()
    {
        RuleFor(x => x.request.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required");

        RuleFor(x => x.request.EndDate)
            .NotEmpty()
            .WithMessage("End date is required");

        RuleFor(x => x.request)
            .Must(x => x.StartDate <= x.EndDate)
            .WithMessage("Start date cannot be later than end date");

        RuleFor(x => x.request.Description)
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.request.Remarks)
            .MaximumLength(1000)
            .WithMessage("Remarks must not exceed 1000 characters");
    }
}