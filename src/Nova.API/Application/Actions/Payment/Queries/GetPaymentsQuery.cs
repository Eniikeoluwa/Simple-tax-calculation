using FluentResults;
using MediatR;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;

namespace Nova.API.Application.Actions.Payment.Queries;

public record GetPaymentsQuery() : MediatR.IRequest<Result<List<PaymentResponse>>>;

public class GetPaymentsQueryHandler : MediatR.IRequestHandler<GetPaymentsQuery, Result<List<PaymentResponse>>>
{
    private readonly IPaymentService _paymentService;

    public GetPaymentsQueryHandler(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<Result<List<PaymentResponse>>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        var paymentsResult = await _paymentService.GetPaymentsForCurrentTenantAsync();
        if (paymentsResult.IsFailed)
            return Result.Fail(paymentsResult.Errors);

        var payments = paymentsResult.Value;
        var responses = payments.Select(payment => new PaymentResponse
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
            Vendor = payment.Vendor != null ? new VendorInfo
            {
                Id = payment.Vendor.Id,
                Name = payment.Vendor.Name,
                Code = payment.Vendor.Code,
                TaxType = payment.Vendor.TaxType,
                VatRate = payment.Vendor.VatRate,
                WhtRate = payment.Vendor.WhtRate
            } : null,
            CreatedByUser = payment.CreatedByUser != null ? new UserInfo
            {
                Id = payment.CreatedByUser.Id,
                FirstName = payment.CreatedByUser.FirstName,
                LastName = payment.CreatedByUser.LastName,
                Email = payment.CreatedByUser.Email
            } : null
        }).ToList();

        return Result.Ok(responses);
    }
}