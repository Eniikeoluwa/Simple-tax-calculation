using Microsoft.AspNetCore.Mvc;
using Nova.API.Application.Services.Common;
using Nova.Contracts.Models;
using Nova.Domain.Utils;
using FluentResults;

namespace Nova.API.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        private static async Task<Result<TResponse>> Send<TRequest, TResponse>(
            IFeatureAction<TRequest, TResponse> action, TRequest request,
            CancellationToken cancellationToken) where TRequest : IRequest<TResponse>
        {
            return await action.Execute(request, cancellationToken);
        }

        protected async Task<IActionResult> SendAction<TRequest>(
            IFeatureAction<TRequest> action, TRequest request)
            where TRequest : IRequest
        {
            try
            {
                return HandleResult(
                    await Send(action, request, HttpContext.RequestAborted)
                );
            }
            catch (Exception ex)
            {
                return Problem(ex);
            }
        }

        protected async Task<IActionResult> SendAction<TRequest, TResponse>(
            IFeatureAction<TRequest, TResponse> action, TRequest request)
            where TRequest : IRequest<TResponse>
        {
            try
            {
                return HandleResult(
                    await Send(action, request, HttpContext.RequestAborted)
                );
            }
            catch (Exception ex)
            {
                return Problem(ex);
            }
        }

        protected IActionResult HandleResult<TResponse>(Result<TResponse> result)
        {
            if (result.IsFailed)
                return Problem(result.Errors);

            if (result.Value is MediaDownloadResult media)
            {
                var stream = media.Stream ?? System.IO.Stream.Null;
                var contentType = media.ContentType ?? "application/octet-stream";
                return File(stream, contentType, media.FileName);
            }

            if (result.Value is BaseResponse baseResponse)
                return Ok(baseResponse);

            return Ok(BaseResponse.CreateSuccess(result.Value));
        }

        protected IActionResult Problem(List<IError> errors)
        {
            var appErrors = errors.Select(AppError.Get).ToList();
            return Problem(appErrors);
        }

        protected IActionResult Problem(List<AppError> errors)
        {
            if (errors.Count == 0)
            {
                return Problem();
            }

            if (errors.All(x => x.Type == ErrorType.Validation))
            {
                return ValidationProblem(errors);
            }

            var errorResponses = errors.Select(e =>
                new ErrorResponse(e.Message, e.Code, e.Type == ErrorType.Validation)
            ).ToList();

            var message = errors.First().Message;

            return Ok(BaseResponse.CreateFailure(message, errorResponses));
        }

        protected IActionResult Problem(AppError error)
        {
            var errorResponse = new ErrorResponse(error.Message, error.Code, error.Type == ErrorType.Validation);
            return Ok(BaseResponse.CreateFailure(error.Message, new List<ErrorResponse> { errorResponse }));
        }

        protected IActionResult Problem(Exception ex)
        {
            var error = new AppError(ex.Message, ErrorType.Unknown, "EXCEPTION");
            return Problem(error);
        }

        private IActionResult ValidationProblem(List<AppError> errors)
        {
            var errorResponses = errors.Select(e =>
                new ErrorResponse(e.Message, e.Code, true)
            ).ToList();

            var message = errors.First().Message;

            return Ok(BaseResponse.CreateFailure(message, errorResponses));
        }
    }
}