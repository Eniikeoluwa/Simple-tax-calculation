using FluentResults;
using FluentValidation;
using Nova.API.Application.Services.Common;
using MediatR;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;
using Nova.Domain.Entities;

namespace Nova.API.Application.Actions.Payment.Commands;

public record CreatePaymentCommand(CreatePaymentRequest request) : MediatR.IRequest<Result<PaymentResponse>>;

public class CreatePaymentCommandHandler : MediatR.IRequestHandler<CreatePaymentCommand, Result<PaymentResponse>>
{
    private readonly IPaymentService _paymentService;

    public CreatePaymentCommandHandler(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<Result<PaymentResponse>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var paymentResult = await _paymentService.CreatePaymentAsync(request.request);
        if (paymentResult.IsFailed)
            return Result.Fail(paymentResult.Errors);

        var payment = paymentResult.Value;

        var response = new PaymentResponse
        {
            Id = payment.Id,
            InvoiceNumber = payment.InvoiceNumber,
            GrossAmount = payment.GrossAmount,
            VatAmount = payment.VatAmount,
            WhtAmount = payment.WhtAmount,
            NetAmount = payment.NetAmount,
            Description = payment.Description,
            Reference = payment.Reference,
            AppliedTaxType = payment.AppliedTaxType,
            AppliedVatRate = payment.AppliedVatRate,
            AppliedWhtRate = payment.AppliedWhtRate,
            InvoiceDate = payment.InvoiceDate,
            PaymentDate = payment.PaymentDate,
            Status = payment.Status,
            Remarks = payment.Remarks,
            VendorId = payment.VendorId,
            BulkScheduleId = payment.BulkScheduleId,
            CreatedByUserId = payment.CreatedByUserId,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt,
            
            // Two-payment system fields
            OriginalInvoiceAmount = payment.OriginalInvoiceAmount,
            PaymentAmount = payment.PaymentAmount,
            PaymentPercentage = payment.OriginalInvoiceAmount > 0 ? (payment.PaymentAmount / payment.OriginalInvoiceAmount * 100) : 0,
            IsPartialPayment = payment.IsPartialPayment,
            IsFinalPayment = payment.IsFinalPayment,
            FirstPaymentId = payment.ParentPaymentId ?? string.Empty,
            TotalAmountPaid = payment.TotalAmountPaid,
            RemainingBalance = payment.GetRemainingBalance()
        };

        if (payment.Vendor != null)
        {
            response.Vendor = new VendorInfo
            {
                Id = payment.Vendor.Id,
                Name = payment.Vendor.Name,
                Code = payment.Vendor.Code,
                TaxType = payment.Vendor.TaxType,
                VatRate = payment.Vendor.VatRate,
                WhtRate = payment.Vendor.WhtRate
            };
        }

        if (payment.CreatedByUser != null)
        {
            response.CreatedByUser = new UserInfo
            {
                Id = payment.CreatedByUser.Id,
                FirstName = payment.CreatedByUser.FirstName,
                LastName = payment.CreatedByUser.LastName,
                Email = payment.CreatedByUser.Email
            };
        }

        return Result.Ok(response);
    }
}

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.request.InvoiceNumber)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Invoice number is required and must not exceed 100 characters");

        RuleFor(x => x.request.GrossAmount)
            .GreaterThan(0)
            .When(x => !x.request.IsPartialPayment)
            .WithMessage("Gross amount must be greater than 0 for full payments");

        RuleFor(x => x.request.Description)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("Description is required and must not exceed 500 characters");

        RuleFor(x => x.request.Reference)
            .MaximumLength(200)
            .WithMessage("Reference must not exceed 200 characters");

        RuleFor(x => x.request.InvoiceDate)
            .NotEmpty()
            .WithMessage("Invoice date is required");

        RuleFor(x => x.request.VendorId)
            .NotEmpty()
            .WithMessage("Vendor ID is required");

        RuleFor(x => x.request.Remarks)
            .MaximumLength(1000)
            .WithMessage("Remarks must not exceed 1000 characters");

        // Two-payment system validation rules
        RuleFor(x => x.request.PartialPercentage)
            .Must(x => x == 50 || x == 70)
            .When(x => x.request.IsPartialPayment && !x.request.IsFinalPayment)
            .WithMessage("First partial payment must be either 50% or 70%");

        RuleFor(x => x.request.PartialPercentage)
            .Must(x => x == 30 || x == 50)
            .When(x => x.request.IsPartialPayment && x.request.IsFinalPayment)
            .WithMessage("Final payment must be either 30% or 50% (remaining amount)");

        RuleFor(x => x.request.FirstPaymentId)
            .NotEmpty()
            .When(x => x.request.IsPartialPayment && x.request.IsFinalPayment)
            .WithMessage("FirstPaymentId is required for final payments");
    }
}