using FluentResults;
using FluentValidation;
using MediatR;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;

namespace Nova.API.Application.Actions.Payment.Commands;

public record UpdatePaymentStatusCommand(string paymentId, UpdatePaymentStatusRequest request) : MediatR.IRequest<Result<bool>>;

public class UpdatePaymentStatusCommandHandler : MediatR.IRequestHandler<UpdatePaymentStatusCommand, Result<bool>>
{
    private readonly IPaymentService _paymentService;

    public UpdatePaymentStatusCommandHandler(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<Result<bool>> Handle(UpdatePaymentStatusCommand command, CancellationToken cancellationToken)
    {
        return await _paymentService.UpdatePaymentStatusAsync(command.paymentId, command.request);
    }
}

public class UpdatePaymentStatusCommandValidator : AbstractValidator<UpdatePaymentStatusCommand>
{
    public UpdatePaymentStatusCommandValidator()
    {
        RuleFor(x => x.paymentId)
            .NotEmpty()
            .WithMessage("Payment ID is required");

        RuleFor(x => x.request.Status)
            .NotEmpty()
            .Must(x => new[] { "Pending", "Approved", "Processed", "Paid", "Cancelled" }.Contains(x))
            .WithMessage("Status must be one of: Pending, Approved, Processed, Paid, Cancelled");

        RuleFor(x => x.request.Remarks)
            .MaximumLength(1000)
            .WithMessage("Remarks must not exceed 1000 characters");
    }
}