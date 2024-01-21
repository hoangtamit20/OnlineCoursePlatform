using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.DTOs.AuthDtos;
using OnlineCoursePlatform.Models.AuthModels;

namespace OnlineCoursePlatform.Helpers
{
    public static class BaseReturnHelper<T>
    {
        public static (int, BaseResponseWithData<LoginResponseDto>) GenerateErrorResponse(
            string errorMessage, int statusCode, string message)
        => (
            statusCode,
            new BaseResponseWithData<LoginResponseDto>()
            {
                IsSuccess = false,
                Message = message,
                Errors = new List<string>() { errorMessage }
            });

        public static (int, BaseResponseWithData<LoginResponseDto>) GenerateSuccessResponse(
            TokenModel tokenModel, string message)
        => (
            StatusCodes.Status200OK,
            new BaseResponseWithData<LoginResponseDto>()
            {
                IsSuccess = true,
                Message = message,
                Data = new LoginResponseDto()
                {
                    AccessToken = tokenModel.AccessToken,
                    RefreshToken = tokenModel.RefreshToken
                }
            }
        );

        public static (int, BaseResponseWithData<T>) GenerateSuccessResponse(
            T data, string message)
        => (
            StatusCodes.Status200OK,
            new BaseResponseWithData<T>()
            {
                IsSuccess = true,
                Message = message,
                Data = data
            }
        );

        public static (int, BaseResponseWithData<T>) GenerateErrorResponse(
            string errorMessage, int statusCode, string message, T? data)
        => (
            statusCode,
            new BaseResponseWithData<T>()
            {
                IsSuccess = false,
                Message = message,
                Data = data,
                Errors = new List<string>() { errorMessage }
            });
    }
}