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

        // Handle errors using MediatR (create a command/query for error handling)
        private async Task<ActionResult<TResponse>> HandleErrors<TResponse>(IEnumerable<IError> errors)
        {
            var appErrors = errors.Select(AppError.Get).ToList();
            var errorCommand = new HandleErrorsCommand<TResponse>(appErrors);
            var errorResult = await _mediator.Send(errorCommand, HttpContext.RequestAborted);
            
            if (errorResult.IsSuccess)
            {
                return Ok(errorResult.Value);
            }
            
            // Fallback to simple BadRequest if error handling fails
            return BadRequest(errors.Select(e => e.Message));
        }

        // Handle exceptions using MediatR
        private async Task<IActionResult> HandleException(Exception ex)
        {
            var exceptionCommand = new HandleExceptionCommand(ex);
            var result = await _mediator.Send(exceptionCommand, HttpContext.RequestAborted);
            
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            
            // Fallback
            return StatusCode(500, "An error occurred while processing your request.");
        }

        // Handle exceptions with typed response using MediatR
        private async Task<ActionResult<TResponse>> HandleException<TResponse>(Exception ex)
        {
            var exceptionCommand = new HandleExceptionCommand<TResponse>(ex);
            var result = await _mediator.Send(exceptionCommand, HttpContext.RequestAborted);
            
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            
            // Fallback
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    // MediatR Commands for error handling
    public class HandleErrorsCommand<TResponse> : IRequest<Result<BaseResponse>>
    {
        public List<AppError> Errors { get; }

        public HandleErrorsCommand(List<AppError> errors)
        {
            Errors = errors;
        }
    }

    public class HandleExceptionCommand : IRequest<Result<BaseResponse>>
    {
        public Exception Exception { get; }

        public HandleExceptionCommand(Exception exception)
        {
            Exception = exception;
        }
    }

    public class HandleExceptionCommand<TResponse> : IRequest<Result<BaseResponse>>
    {
        public Exception Exception { get; }

        public HandleExceptionCommand(Exception exception)
        {
            Exception = exception;
        }
    }
}