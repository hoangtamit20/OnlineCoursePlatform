using System.Net;
using System.Security.Cryptography;
using System.Text;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Configurations;
using OnlineCoursePlatform.Constants;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.Data.Entities.Order;
using OnlineCoursePlatform.Data.Entities.PaymentCollection;
using OnlineCoursePlatform.DTOs.PaymentDtos;

namespace OnlineCoursePlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly OnlineCoursePlatformDbContext _context;
        private readonly VNPayConfig _vnpayConfig;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<PaymentController> _logger;
        // private readonly IHubContext<NotificationHub> _hubContext;

        public PaymentController(
            OnlineCoursePlatformDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IOptions<VNPayConfig> vnpayConfig,
            UserManager<AppUser> userManager,
            ILogger<PaymentController> logger
            // IHubContext<NotificationHub> hubContext
            )
        => (_context, _httpContextAccessor, _vnpayConfig, _userManager, _logger)
        = (context, httpContextAccessor, vnpayConfig.Value, userManager, logger);

        /// <summary>
        /// Payment order with VnPay, Momo, ZaloPay (Authorize)
        /// </summary>
        /// <param name="paymentInfoDto"></param>
        /// <returns></returns>
        /// <remarks>
        ///     POST : You can get the payment destination id from api: /api/v1/payments/paymentdestinations corresponding to the payment method you want
        /// {
        ///     "paymentContent": "THANH TOAN DON HANG 0001",
        ///     "paymentCurrency": "VND",
        ///     "requiredAmount": 200000,
        ///     "paymentLanguage": "VN",
        ///     "merchantId": "MerchantVNPay"
        ///     "paymentDestinationId": "df023f81-8346-4f40-9b12-cca7eb5dec42",
        ///     "orderId": guid string
        /// }
        /// </remarks>
        [HttpPost("/api/v1/payments/create")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponseWithData<PaymentLinkDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentInfoDto paymentInfoDto)
        {
            var paymentRequestDto = new PaymentInfoRequestDto()
            {
                OrderId = paymentInfoDto.OrderId,
                PaymentContent = paymentInfoDto.PaymentContent,
                RequiredAmount = paymentInfoDto.RequiredAmount
            };
            if (ModelState.IsValid)
            {
                var order = await _context.OrderCourses.FirstOrDefaultAsync(od => od.Id == paymentRequestDto.OrderId);
                if (order is not null && order.Status == OrderStatus.Success)
                {
                    return BadRequest(new BaseResponseWithData<PaymentLinkDto>()
                    {
                        Errors = new List<string>() { $"The order {paymentRequestDto.OrderId} was paymented" },
                        IsSuccess = false,
                        Message = "Create payment failed"
                    });
                }
                var result = new BaseResponseWithData<PaymentLinkDto>();
                using (var _transaction = await _context.Database.BeginTransactionAsync())
                {
                    var payment = new Payment()
                    {
                        ExpireDate = paymentRequestDto.ExpireDate,
                        MerchantId = paymentRequestDto.MerchantId,
                        OrderCourseId = paymentRequestDto.OrderId,
                        PaymentContent = paymentRequestDto.PaymentContent,
                        PaymentCurrency = paymentRequestDto.PaymentCurrency,
                        PaymentDate = paymentRequestDto.PaymentDate,
                        PaymentDestinationId = paymentRequestDto.PaymentDestinationId,
                        PaymentLanguage = paymentRequestDto.PaymentLanguage,
                        RequiredAmount = paymentRequestDto.RequiredAmount
                    };
                    try
                    {
                        var paymentDest = await _context.PaymentDestinations.FirstOrDefaultAsync(t => t.Id == paymentRequestDto.PaymentDestinationId);
                        Merchant? merchant = null!;
                        if (string.IsNullOrEmpty(paymentRequestDto.MerchantId))
                        {
                            merchant = await _context.Merchants.FirstOrDefaultAsync(m =>
                            m.MerchantName!.ToLower().Contains(paymentDest!.DesShortName!.ToLower()));
                        }
                        else
                        {
                            merchant = await _context.Merchants.FindAsync(paymentRequestDto.MerchantId);
                        }
                        

                        // payment.MerchantId = paymentRequestDto.MerchantId ??
                        //     (paymentDest?.DesShortName?.ToLower() == PaymentMethodConstant.VNPAY.ToLower()
                        //     ? 1 : paymentDest?.DesShortName?.ToLower() == PaymentMethodConstant.MOMO.ToLower() ? 2 : 3);
                        payment.MerchantId = merchant?.Id;
                        _context.Payments.Add(payment);
                        order!.Status = OrderStatus.Progressing;
                        _context.OrderCourses.Update(order);
                        await _context.SaveChangesAsync();


                        // Tạo ra một chuỗi nối các thông tin trên theo thứ tự quy định
                        string data = $"{payment.MerchantId}{payment.OrderCourseId}{payment.RequiredAmount}{payment.PaymentCurrency}{_vnpayConfig.ReturnUrl}";

                        // Tạo ra một đối tượng HMACSHA256 với khóa bí mật
                        HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_vnpayConfig.HashSecret));

                        // Tính toán chuỗi băm của data
                        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));

                        // Chuyển đổi chuỗi băm thành chuỗi hexa
                        string signValue = BitConverter.ToString(hash).Replace("-", "").ToLower();


                        var paymentSignature = new PaymentSignature()
                        {
                            SignValue = signValue,
                            SignDate = DateTime.UtcNow,
                            SignOwn = paymentRequestDto.MerchantId,
                            PaymentId = payment.Id,
                            IsValid = true
                        };
                        _context.PaymentSignatures.Add(paymentSignature);
                        await _context.SaveChangesAsync();

                        // choice method for payment
                        var paymentUrl = string.Empty;
                        switch ((await GetPaymentDestinationShortName(payment.PaymentDestinationId)).ToUpper())
                        {
                            //process for vnpay
                            case PaymentMethodConstant.VNPAY:
                                {
                                    var vnpayRequest = new VnPayRequestDto(
                                        _vnpayConfig.Version,
                                        _vnpayConfig.TmnCode,
                                        DateTime.UtcNow,
                                        _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty,
                                        paymentRequestDto.RequiredAmount ?? 0,
                                        paymentRequestDto.PaymentCurrency ?? string.Empty,
                                        "other",
                                        paymentRequestDto.PaymentContent,
                                        _vnpayConfig.ReturnUrl,
                                        payment.Id
                                    );
                                    paymentUrl = vnpayRequest.GetLink(_vnpayConfig.PaymentUrl, _vnpayConfig.HashSecret);
                                    break;
                                }
                            default:
                                await _transaction.RollbackAsync();
                                break;
                        }
                        await _transaction.CommitAsync();
                        return Ok(new BaseResponseWithData<PaymentLinkDto>()
                        {
                            IsSuccess = true,
                            Message = result.Message,
                            Data = new PaymentLinkDto()
                            {
                                PaymentId = payment.Id,
                                PaymentUrl = paymentUrl
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        await _transaction.RollbackAsync();
                        return StatusCode(500, new BaseResponseWithData<PaymentLinkDto>()
                        { 
                            Errors = new List<string>() { $"Server error - {ex.Message}" },
                            IsSuccess = false,
                            Message = "Create payment failed"
                        });
                    }

                }
            }
            return BadRequest(new BaseResponseWithData<PaymentLinkDto>()
            {
                Errors = ModelState.SelectMany(x => x.Value!.Errors.Select(p => p.ErrorMessage)).ToList(),
                IsSuccess = false,
                Message = "Create payment failed"
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vnPayResponseDto"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/api/v1/payments/vnpay-return")]
        [ProducesResponseType(typeof(RedirectResult), (int)HttpStatusCode.Found)]
        public async Task<IActionResult> VnPayReturn([FromQuery] VnPayResponseDto vnPayResponseDto)
        {
            string returnUrl = string.Empty;
            var returnModel = new PaymentReturnDto();
            try
            {
                var isValidSignature = vnPayResponseDto.IsValidSignature(_vnpayConfig.HashSecret);
                if (isValidSignature)
                {
                    var payment = (await _context.Payments.FindAsync(vnPayResponseDto.vnp_TxnRef)).Adapt<PaymentDto>();
                    if (payment is not null)
                    {
                        var merchant = (await _context.Merchants.FindAsync(payment.MerchantId)).Adapt<MerchantInfoDto>();
                        //TODO: create returnUrl
                        returnUrl = merchant?.MerchantReturnUrl ?? string.Empty;
                    }
                    else
                    {
                        returnModel.PaymentStatus = "11";
                        returnModel.PaymentMessage = "Can't find payment at payment service";
                    }

                    if (vnPayResponseDto.vnp_ResponseCode == "00")
                    {
                        returnModel.PaymentStatus = "00";
                        returnModel.PaymentId = payment!.Id;
                        //TODO: Make signature
                        var paymenSign = (await _context.PaymentSignatures
                            .FirstOrDefaultAsync(p => p.PaymentId == vnPayResponseDto.vnp_TxnRef))?.SignValue;
                        returnModel.Signature = paymenSign;
                    }
                    else
                    {
                        returnModel.PaymentStatus = "10";
                        returnModel.PaymentMessage = "Payment process failed";
                    }
                }
                else
                {
                    returnModel.PaymentStatus = "99";
                    returnModel.PaymentMessage = $"Invalid signature in response!";
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponseWithData<bool>()
                {
                    Errors = new List<string>() { $"Failed : {ex.Message} " },
                    IsSuccess = false,
                    Message = "Payment return failed"
                });
            }
            // var b = returnUrl;
            if (returnUrl.EndsWith("/"))
                returnUrl = returnUrl.Remove(returnUrl.Length - 1, 1);
            // var a = $"{returnUrl}?{returnModel.ToQueryString()}";
            // System.Console.WriteLine("daiuhdauihd");
            var redirectUrl = $"{returnUrl}?{returnModel.ToQueryString()}";
            return Redirect(redirectUrl);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vnPayIpnResponseDto"></param>
        /// <returns></returns>
        /// <remarks>
        ///     GET:
        /// {
        ///     9704198526191432198
        ///     NGUYEN VAN A
        ///     07/15
        ///     123456
        /// }
        /// </remarks>

        [HttpGet]
        [Route("/api/v1/payments/check-vnpay-payment")]
        public async Task<IActionResult> CheckPayment([FromQuery] VnPayIpnResponseDto vnPayIpnResponseDto)
        {
            try
            {
                // check valid signature
                var isValidSignature = vnPayIpnResponseDto.IsValidSignature(_vnpayConfig.HashSecret);
                if (isValidSignature)
                {
                    // get payment required
                    var payment = _context.Payments.Find(vnPayIpnResponseDto.vnp_TxnRef);
                    if (payment != null)
                    {
                        // check amount valid
                        if (payment.RequiredAmount == (vnPayIpnResponseDto.vnp_Amount / 100))
                        {
                            // check payment status
                            if (payment.PaymentStatus != "0")
                            {
                                string message = string.Empty;
                                string status = string.Empty;
                                if (vnPayIpnResponseDto.vnp_ResponseCode == "00" &&
                                    vnPayIpnResponseDto.vnp_TransactionStatus == "00")
                                {
                                    status = "0";
                                    message = "Tran success";
                                }
                                else
                                {
                                    status = "-1";
                                    message = "Tran error";
                                }

                                // create payment trans
                                using (var _transaction = _context.Database.BeginTransaction())
                                {
                                    try
                                    {
                                        var paymentTransDto = new CreatePaymentTransDto()
                                        {
                                            PaymentId = vnPayIpnResponseDto.vnp_TxnRef,
                                            TranMessage = message,
                                            TranDate = DateTime.UtcNow,
                                            TranPayload = JsonConvert.SerializeObject(vnPayIpnResponseDto),
                                            TranStatus = status,
                                            TranAmount = vnPayIpnResponseDto.vnp_Amount / 100
                                        };

                                        var paymentTrans = paymentTransDto.Adapt<PaymentTransaction>();
                                        _context.PaymentTransactions.Add(paymentTrans);
                                        await _context.SaveChangesAsync();

                                        // update payment
                                        payment.PaymentLastMessage = paymentTrans.TranMessage;
                                        payment.PaidAmount = (_context.PaymentTransactions
                                            .Where(pt => pt.PaymentId == payment.Id && pt.TranStatus == "0")
                                            .Sum(pt => pt.TranAmount));
                                        payment.PaymentStatus = paymentTrans.TranStatus;
                                        payment.LastUpdateAt = DateTime.Now;

                                        // update status for Payment
                                        _context.Entry<Payment>(payment).State = EntityState.Modified;
                                        await _context.SaveChangesAsync();

                                        // update status for Order
                                        var order = await _context.OrderCourses.FindAsync(payment.OrderCourseId);
                                        order!.Status = OrderStatus.Success;
                                        _context.Entry<OrderCourse>(order).State = EntityState.Modified;
                                        await _context.SaveChangesAsync();
                                        // update for course interactions
                                        var orderItems = await _context.OrderDetails.Where(od => od.OrderCourseId == order.Id)
                                            .ToListAsync();
                                        if (!orderItems.IsNullOrEmpty())
                                        {
                                            var interactionCoursesCreate = new List<UserCourseInteraction>();
                                            var interactionCoursesUpdate = new List<UserCourseInteraction>();

                                            orderItems.ForEach(od => {
                                                var interactionCourse = _context.UserCourseInteractions.Where(uci => uci.CourseId == od.CourseId
                                                    && uci.UserId == order.UserId).FirstOrDefault();
                                                if (interactionCourse == null)
                                                {
                                                    interactionCourse = new UserCourseInteraction()
                                                    {
                                                        UserId = order.UserId,
                                                        CourseId = od.CourseId,
                                                        PurchaseScore = 1,
                                                    };
                                                    interactionCoursesCreate.Add(interactionCourse);
                                                }
                                                else
                                                {
                                                    interactionCourse.PurchaseScore += 1;
                                                    interactionCoursesUpdate.Add(interactionCourse);
                                                }
                                            });
                                            if (!interactionCoursesUpdate.IsNullOrEmpty())
                                            {
                                                _context.UserCourseInteractions.UpdateRange(interactionCoursesUpdate);
                                            }
                                            if (!interactionCoursesCreate.IsNullOrEmpty())
                                            {
                                                _context.UserCourseInteractions.AddRange(interactionCoursesUpdate);
                                            }
                                            await _context.SaveChangesAsync();
                                        }

                                        var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == order.UserId);
                                        if (currentUser is null)
                                        {
                                            _transaction.Rollback();
                                            return BadRequest(new { RspCode = "99", Message = "Tran error" });
                                        }

                                        //remove all product items were payment in customer's cart
                                        var orderDetailOfCurrentOrder = await _context.OrderDetails
                                            .Where(odl => odl.OrderCourseId == order.Id)
                                            .ToListAsync();
                                        foreach (var odDetail in orderDetailOfCurrentOrder)
                                        {
                                            var cartItem = await _context.CartItems
                                                .Include(c => c.Cart)
                                                .FirstOrDefaultAsync(cartItem =>
                                                    cartItem.CourseId == odDetail.CourseId
                                                    && cartItem.Cart.UserId == currentUser.Id);
                                            if (cartItem is not null)
                                            {
                                                _context.CartItems.Remove(cartItem);
                                            }
                                        }
                                        await _context.SaveChangesAsync();
                                        // get order comfirmed info
                                        var orderConfirmed = order.Adapt<OrderConfirmedDto>();
                                        orderConfirmed.Name = currentUser.Name;
                                        orderConfirmed.Email = currentUser.Email!;
                                        _transaction.Commit();

                                        // send message to clien when the order was payment successed;
                                        // await _hubContext.Clients.All.SendAsync(SignalRConstant.ReceiveNotification,
                                        //     $"Customer {orderConfirmed.Name} was payment order {orderConfirmed.Id} successed!");
                                        // await _hubContext.Clients.All.SendAsync(SignalRConstant.ReceiveOrderConfirmed, orderConfirmed);
                                        System.Console.WriteLine("Every thing is OK!");
                                        // return for VnPay
                                        return Ok(new { RspCode = "00", Message = "Confirm Success" });
                                    }
                                    catch (Exception ex)
                                    {
                                        _transaction.Rollback();
                                        return BadRequest(new { RspCode = "99", Message = $"{ex.Message}" });
                                    }
                                }
                            }
                            else
                            {
                                return BadRequest(new { RspCode = "02", Message = "Order already confirmed" });
                            }
                        }
                        else
                        {
                            return BadRequest(new { RspCode = "04", Message = "Invalid amount" });
                        }
                    }
                    else
                    {
                        return BadRequest(new { RspCode = "01", Message = "Order not found" });
                    }
                }
                else
                {
                    return BadRequest(new { RspCode = "97", Message = "Invalid Signature" });
                }
            }
            catch (Exception ex)
            {
                // TODO: process when exception
                return BadRequest(new
                {
                    RspCode = "99",
                    Message = $"Input required data - {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpGet]
        [Route("/api/v1/payments/{id}")]
        [ProducesResponseType(typeof(BaseResponseWithData<PaymentDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BaseResponseWithData<PaymentDto>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment is null)
                return NotFound(new BaseResponseWithData<PaymentDto>()
                { 
                    Errors = new List<string>() { $"Payment with Id : {id} not found!" },
                    Message = "Get payment by id failed",
                    IsSuccess = false
                });
            return Ok(new BaseResponseWithData<PaymentDto>()
            {
                IsSuccess = true,
                Message = $"Get payment by id : {id}",
                Data = payment.Adapt<PaymentDto>()
            });
        }


        [HttpGet("/api/v1/payments/paymentdestinations")]
        public async Task<IActionResult> GetPaymentDestination()
        {
            return Ok(await _context.PaymentDestinations.ToListAsync());
        }


        private async Task<string> GetPaymentDestinationShortName(string paymentDesId)
            => (await _context.PaymentDestinations.FindAsync(paymentDesId))!.DesShortName!;
    }
}