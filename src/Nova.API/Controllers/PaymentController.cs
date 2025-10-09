using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nova.API.Application.Actions.Payment.Commands;
using Nova.API.Application.Actions.Payment.Queries;
using Nova.API.Controllers;
using Nova.Contracts.Models;
using MediatR;

namespace Nova.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PaymentController : BaseController
{
    public PaymentController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost("create")]
    public async Task<ActionResult<PaymentResponse>> CreatePayment(
        [FromBody] CreatePaymentRequest request)
    {
        var command = new CreatePaymentCommand(request);
        return await SendCommand<CreatePaymentCommand, PaymentResponse>(command);
    }

    [HttpGet("list")]
    public async Task<ActionResult<List<PaymentResponse>>> GetPayments()
    {
        var query = new GetPaymentsQuery();
        return await SendQuery<GetPaymentsQuery, List<PaymentResponse>>(query);
    }

    [HttpPatch("{paymentId}/status")]
    public async Task<ActionResult<bool>> UpdatePaymentStatus(
        string paymentId,
        [FromBody] UpdatePaymentStatusRequest request)
    {
        var command = new UpdatePaymentStatusCommand(paymentId, request);
        return await SendCommand<UpdatePaymentStatusCommand, bool>(command);
    }
}
