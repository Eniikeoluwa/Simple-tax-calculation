using FluentResults;
using FluentValidation;
using Nova.API.Application.Services.Common;
using MediatR;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;

namespace Nova.API.Application.Actions.Payment.Commands;

public record ApprovePaymentCommand(string PaymentId, ApprovePaymentRequest Request) : IRequest<Result<bool>>;

public class ApprovePaymentCommandHandler : IRequestHandler<ApprovePaymentCommand, Result<bool>>
{
    private readonly IPaymentService _paymentService;

    public ApprovePaymentCommandHandler(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<Result<bool>> Handle(ApprovePaymentCommand request, CancellationToken cancellationToken)
    {
        return await _paymentService.ApprovePaymentAsync(request.PaymentId, request.Request);
    }
}

public class ApprovePaymentCommandValidator : AbstractValidator<ApprovePaymentCommand>
{
    public ApprovePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("Payment ID is required");

        RuleFor(x => x.Request.Remarks)
            .MaximumLength(500)
            .WithMessage("Remarks cannot exceed 500 characters");
    }
}