using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Constants;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.Data.Entities.Order;
using OnlineCoursePlatform.Data.Entities.PaymentCollection;
using OnlineCoursePlatform.DTOs.PaymentDtos.MerchantDtos;
using OnlineCoursePlatform.DTOs.PaymentDtos.PaymentDestinationDtos;
using OnlineCoursePlatform.Helpers;

namespace OnlineCoursePlatform.Services.PaymentServices
{
    public class PaymentService : IPaymentService
    {
        private readonly OnlineCoursePlatformDbContext _dbContext;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly ILogger<PaymentService> _logger;
        private readonly UserManager<AppUser> _userManager;

        public PaymentService(OnlineCoursePlatformDbContext dbContext,
            ILogger<PaymentService> logger,
            IHttpContextAccessor httpAccessor,
            UserManager<AppUser> userManager)
        {
            _dbContext = dbContext;
            _httpAccessor = httpAccessor;
            _logger = logger;
            _userManager = userManager;
        }


        public async Task<(int statusCode, BaseResponseWithData<IEnumerable<PaymentDestination>> result)> GetAllPaymentDestinationAsync()
        {
            return BaseReturnHelper<IEnumerable<PaymentDestination>>.GenerateSuccessResponse(
                data: await _dbContext.PaymentDestinations.ToListAsync(),
                message: "Get payment destination successfully");
        }

        public async Task<(int statusCode, BaseResponseWithData<PaymentDestination> result)> GetPaymentDesinationAsync(string paymentDesId)
        {
            var currentPaymentDes = await _dbContext.PaymentDestinations.FindAsync(paymentDesId);
            if (currentPaymentDes == null)
            {
                return BaseReturnHelper<PaymentDestination>.GenerateErrorResponse(
                    errorMessage: $"Payment destination with id '{paymentDesId}' not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    message: "Get payment destination failed",
                    data: null);
            }

            return BaseReturnHelper<PaymentDestination>.GenerateSuccessResponse(
                data: currentPaymentDes,
                message: "Get payment destination successfully");
        }


        // [HttpPost("/api/v1/payments/addpaymentdestination")]
        // [Authorize(Roles = RolesConstant.Admin)]
        public async Task<(int statusCode, BaseResponseWithData<PaymentDestination> result)> 
            CreatePaymentDestinationAsync(CreatePaymentDestinationRequestDto requestDto)
        {
            var currentUser = await GetCurrentUserAsync();
            var paymentDes = new PaymentDestination()
            {
                DesName = requestDto.DesName,
                DesShortName = requestDto.DesShortName,
                CreateAt = DateTime.UtcNow,
                CreateBy = currentUser == null ? null : currentUser.Name,
                IsActive = true,
            };
            _dbContext.PaymentDestinations.Add(paymentDes);
            try
            {
                await _dbContext.SaveChangesAsync();
                return BaseReturnHelper<PaymentDestination>.GenerateSuccessResponse(data: paymentDes, 
                    message: "Create payment destination successfully");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return BaseReturnHelper<PaymentDestination>.GenerateErrorResponse(
                    errorMessage: "An error occured while create payment destination",
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "Create payment desination failed",
                    data: null);
            }
        }


        public async Task<(int statusCode, BaseResponseWithData<Merchant> result)> CreateMerchantAsync(CreateMerchantRequestDto requestDto)
        {
            var merchant = requestDto.Adapt<Merchant>();
            _dbContext.Merchants.Add(merchant);
            try
            {
                await _dbContext.SaveChangesAsync();
                return BaseReturnHelper<Merchant>.GenerateSuccessResponse(data: merchant, 
                    message: "Create merchant successfully");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return BaseReturnHelper<Merchant>.GenerateErrorResponse(
                    errorMessage: "An error occured while create payment destination",
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "Create merchant failed",
                    data: null);
            }
        }

        public async Task<(int statusCode, BaseResponseWithData<IEnumerable<Merchant>> result)> GetAllMerchantsAsync()
        {
            return BaseReturnHelper<IEnumerable<Merchant>>.GenerateSuccessResponse(
                data: await _dbContext.Merchants.ToListAsync(),
                message: "Get merchants successfully");
        }

        public async Task<(int statusCode, BaseResponseWithData<Merchant> result)> GetMerchantAsync(string merchantId)
        {
            var currentMerchant = await _dbContext.Merchants.FindAsync(merchantId);
            if (currentMerchant == null)
            {
                return BaseReturnHelper<Merchant>.GenerateErrorResponse(
                    errorMessage: $"Merchant with id '{merchantId}' not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    message: "Get Merchant failed",
                    data: null);
            }

            return BaseReturnHelper<Merchant>.GenerateSuccessResponse(
                data: currentMerchant,
                message: "Get Merchant successfully");
        }




        public async Task<AppUser?> GetCurrentUserAsync()
        {
            ClaimsPrincipal? currentClaimsPrincipal = _httpAccessor.HttpContext?.User;
            if (currentClaimsPrincipal != null)
            {
                return await _userManager.GetUserAsync(principal: currentClaimsPrincipal);
            }
            return null;
        }
    }
}