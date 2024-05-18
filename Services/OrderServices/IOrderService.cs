using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.DTOs.OrderDtos;

namespace OnlineCoursePlatform.Services.OrderServices
{
    public interface IOrderService
    {
        Task<(int statusCode, BaseResponseWithData<CreateOrderResponseDto> result)>
            CreateOrderAsync(CreateOrderRequestDto requestDto);

        Task<(int statusCode, BaseResponseWithData<OrderDetailDto> result)> GetOrderAsync(string orderId);
        Task<(int statusCode, BaseResponseWithData<List<OrderInfoResponseDto>> result)> GetMyOrdersAsync();
        Task<(int statusCode, BaseResponseWithData<List<OrderInfoForAdminDto>> result)> GetOrdersAsync();
    }
}