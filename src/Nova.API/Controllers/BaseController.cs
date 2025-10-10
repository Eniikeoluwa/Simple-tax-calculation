using Microsoft.AspNetCore.Mvc;
using Nova.Contracts.Models;
using Nova.Domain.Utils;
using FluentResults;
using MediatR;

namespace Nova.API.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        private readonly IMediator _mediator;

        protected BaseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // For commands without return values
        protected async Task<IActionResult> SendCommand<TRequest>(TRequest request)
            where TRequest : IRequest<Result<Unit>>
        {
            try
            {
                var result = await _mediator.Send(request, HttpContext.RequestAborted);
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                return await HandleException(ex);
            }
        }

        // For commands with return values
        protected async Task<ActionResult<TResponse>> SendCommand<TRequest, TResponse>(TRequest request)
            where TRequest : IRequest<Result<TResponse>>
        {
            try
            {
                var result = await _mediator.Send(request, HttpContext.RequestAborted);
                if (result.IsSuccess)
                {
                    // Add null check for debugging
                    if (result.Value == null)
                    {
                        return Ok(new { message = "Success but value is null", data = (TResponse?)default });
                    }
                    return Ok(result.Value);
                }

                return await HandleErrors<TResponse>(result.Errors);
            }
            catch (Exception ex)
            {
                return await HandleException<TResponse>(ex);
            }
        }

        // For queries
        protected async Task<ActionResult<TResponse>> SendQuery<TRequest, TResponse>(TRequest request)
            where TRequest : IRequest<Result<TResponse>>
        {
            try
            {
                var result = await _mediator.Send(request, HttpContext.RequestAborted);
                if (result.IsSuccess)
                {
                    return Ok(result.Value);
                }

                return await HandleErrors<TResponse>(result.Errors);
            }
            catch (Exception ex)
            {
                return await HandleException<TResponse>(ex);
            }
        }

        // Generic method for handling Result<T>
        private IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            return BadRequest(result.Errors.Select(e => e.Message));
        }

        // Handle errors directly without MediatR
        private Task<ActionResult<TResponse>> HandleErrors<TResponse>(IEnumerable<IError> errors)
        {
            var appErrors = errors.Select(AppError.Get).ToList();
            
            if (appErrors.Count == 0)
            {
                var unknownErrorResponse = BaseResponse.CreateFailure("Unknown error occurred", new List<ErrorResponse>());
                return Task.FromResult<ActionResult<TResponse>>(BadRequest(unknownErrorResponse));
            }

            if (appErrors.All(x => x.Type == ErrorType.Validation))
            {
                return Task.FromResult(HandleValidationErrors<TResponse>(appErrors));
            }

            var errorResponses = appErrors.Select(e =>
                new ErrorResponse(e.Message, e.Code, e.Type == ErrorType.Validation)
            ).ToList();

            var message = appErrors.First().Message;
            var response = BaseResponse.CreateFailure(message, errorResponses);
            
            return Task.FromResult<ActionResult<TResponse>>(BadRequest(response));
        }

        private ActionResult<TResponse> HandleValidationErrors<TResponse>(List<AppError> errors)
        {
            var errorResponses = errors.Select(e =>
                new ErrorResponse(e.Message, e.Code, true)
            ).ToList();

            var message = errors.First().Message;
            var response = BaseResponse.CreateFailure(message, errorResponses);
            
            return BadRequest(response);
        }

        // Handle exceptions directly without MediatR
        private Task<IActionResult> HandleException(Exception ex)
        {
            var error = new AppError(ex.Message, ErrorType.Unknown, "EXCEPTION");
            var errorResponse = new ErrorResponse(error.Message, error.Code, error.Type == ErrorType.Validation);
            var response = BaseResponse.CreateFailure(error.Message, new List<ErrorResponse> { errorResponse });
            
            return Task.FromResult<IActionResult>(StatusCode(500, response));
        }

        // Handle exceptions with typed response directly without MediatR
        private Task<ActionResult<TResponse>> HandleException<TResponse>(Exception ex)
        {
            var error = new AppError(ex.Message, ErrorType.Unknown, "EXCEPTION");
            var errorResponse = new ErrorResponse(error.Message, error.Code, error.Type == ErrorType.Validation);
            var response = BaseResponse.CreateFailure(error.Message, new List<ErrorResponse> { errorResponse });
            
            return Task.FromResult<ActionResult<TResponse>>(StatusCode(500, response));
        }
    }
}