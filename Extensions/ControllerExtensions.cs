using Microsoft.AspNetCore.Mvc;
using UserApi.Common;
using UserApi.DTOs.Responses;

namespace UserApi.Extensions;

public static class ControllerExtensions
{
    public static ActionResult<ApiResponse<T>> ToActionResult<T>(
        this ControllerBase controller,
        ServiceResult<T> result,
        int successStatusCode = StatusCodes.Status200OK)
    {
        if (!result.Success || result.Data == null)
        {
            return result.ErrorType switch
            {
                ServiceErrorType.NotFound => controller.NotFound(
                    new ApiResponse<T>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = result.Message
                    }
                ),

                ServiceErrorType.Conflict => controller.Conflict(
                    new ApiResponse<T>
                    {
                        StatusCode = StatusCodes.Status409Conflict,
                        Message = result.Message
                    }
                ),

                ServiceErrorType.Forbidden => controller.StatusCode(
                    StatusCodes.Status403Forbidden,
                    new ApiResponse<T>
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = result.Message
                    }
                ),

                _ => controller.BadRequest(
                    new ApiResponse<T>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = result.Message
                    }
                )
            };
        }

        return controller.StatusCode(
            successStatusCode,
            new ApiResponse<T>
            {
                StatusCode = successStatusCode,
                Message = result.Message,
                Content = result.Data
            }
        );
    }
}