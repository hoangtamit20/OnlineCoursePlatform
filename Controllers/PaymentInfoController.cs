using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCoursePlatform.Constants;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.DTOs.PaymentDtos.MerchantDtos;
using OnlineCoursePlatform.DTOs.PaymentDtos.PaymentDestinationDtos;
using OnlineCoursePlatform.Services.PaymentServices;

namespace OnlineCoursePlatform.Controllers
{
    [ApiController]
    [Authorize(Roles = RolesConstant.Admin)]
    [Route("api/[controller]")]
    public class PaymentInfoController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentInfoController> _logger;
        private readonly OnlineCoursePlatformDbContext _dbContext;

        public PaymentInfoController(OnlineCoursePlatformDbContext dbContext,
            ILogger<PaymentInfoController> logger,
            IPaymentService paymentService)
        {
            _paymentService = paymentService;
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet("/api/v1/payments/getallpaymentdestinations")]
        public async Task<IActionResult> GetAllPaymentDestinations()
        {
            var result = await _paymentService.GetAllPaymentDestinationAsync();
            return StatusCode(statusCode: result.statusCode, result.result);
        }

        [HttpGet("/api/v1/payments/getpaymentdestination/{paymentDesId}")]
        public async Task<IActionResult> GetPaymentDesById(string paymentDesId)
        {
            var result = await _paymentService.GetPaymentDesinationAsync(paymentDesId);
            return StatusCode(statusCode: result.statusCode, value: result.result);
        }

        [HttpPost("/api/v1/payments/addpaymentdestination")]
        public async Task<IActionResult> CreatePaymentDesination(CreatePaymentDestinationRequestDto requestDto)
        {
            var result = await _paymentService.CreatePaymentDestinationAsync(requestDto);
            return StatusCode(statusCode: result.statusCode, value: result.result);
        }

        [HttpGet("/api/v1/payments/getallmerchants")]
        public async Task<IActionResult> GetAllMerchants()
        {
            var result = await _paymentService.GetAllMerchantsAsync();
            return StatusCode(statusCode: result.statusCode, result.result);
        }

        [HttpGet("/api/v1/payments/getmerchant/{merchantId}")]
        public async Task<IActionResult> GetMerchantById(string merchantId)
        {
            var result = await _paymentService.GetMerchantAsync(merchantId);
            return StatusCode(statusCode: result.statusCode, result.result);
        }

        [HttpPost("/api/v1/payments/createmerchant")]
        public async Task<IActionResult> CreateMerchant(CreateMerchantRequestDto requestDto)
        {
            var result = await _paymentService.CreateMerchantAsync(requestDto);
            return StatusCode(statusCode: result.statusCode, value: result.result);
        }
    }
}