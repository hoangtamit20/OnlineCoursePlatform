using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Data.Entities.Order;
using OnlineCoursePlatform.Data.Entities.PaymentCollection;
using OnlineCoursePlatform.DTOs.PaymentDtos.MerchantDtos;
using OnlineCoursePlatform.DTOs.PaymentDtos.PaymentDestinationDtos;

namespace OnlineCoursePlatform.Services.PaymentServices
{
    public interface IPaymentService
    {
        Task<(int statusCode, BaseResponseWithData<PaymentDestination> result)> 
            CreatePaymentDestinationAsync(CreatePaymentDestinationRequestDto requestDto);
        Task<(int statusCode, BaseResponseWithData<IEnumerable<PaymentDestination>> result)> GetAllPaymentDestinationAsync();
        Task<(int statusCode, BaseResponseWithData<PaymentDestination> result)> GetPaymentDesinationAsync(string paymentDesId);
        Task<(int statusCode, BaseResponseWithData<Merchant> result)> CreateMerchantAsync(CreateMerchantRequestDto requestDto);
        Task<(int statusCode, BaseResponseWithData<IEnumerable<Merchant>> result)> GetAllMerchantsAsync();
        Task<(int statusCode, BaseResponseWithData<Merchant> result)> GetMerchantAsync(string merchantId);

    }
}