using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Constants;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.Data.Entities.Order;
using OnlineCoursePlatform.DTOs.OrderDtos;
using OnlineCoursePlatform.Services.OrderServices;

namespace OnlineCoursePlatform.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OnlineCoursePlatformDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;
        // private readonly IHubContext<NotificationHub> _hubContext;

        public OrderController(
            OnlineCoursePlatformDbContext context,
            UserManager<AppUser> userManager,
            ILogger<OrderController> logger,
            // IHubContext<NotificationHub> hubContext,
            IOrderService orderService)
        {
            _context = context;
            _userManager = userManager;
            // _hubContext = hubContext;
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// This api to process create order course
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        [HttpPost("/api/v1/orders/create")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponseWithData<CreateOrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateOrder(CreateOrderRequestDto requestDto)
        {
            var result = await _orderService.CreateOrderAsync(requestDto);
            return StatusCode(statusCode: result.statusCode, value: result.result);
        }

        [HttpGet("/api/v1/orders/{courseId}")]
        [Authorize]
        public async Task<IActionResult> GetOrder(string courseId)
        {
            var result = await _orderService.GetOrderAsync(courseId);
            return StatusCode(statusCode: result.statusCode, value: result.result);
        }


        /// <summary>
        /// Set status for order by id (Admin)
        /// </summary>
        /// <param name="id">Order Id</param>
        /// <param name="orderStatus">Bao gồm các value : {InProgress; Confirmed; Shipping; Success; Canceled}</param>
        /// <returns></returns>
        [HttpPut]
        [Route("/api/v1/orders/{id}/set-order-status")]
        [Authorize(Roles = RolesConstant.Admin)]
        public async Task<IActionResult> SetOrderStatus(string id, int orderStatus)
        {
            var orderExists = await _context.OrderCourses.FindAsync(id);
            if (orderExists is null)
            {
                return NotFound(new BaseResponseWithData<bool>()
                {
                    Errors = new List<string>() { $"Order with Id : {id} not found" },
                    IsSuccess = false,
                    Message = "Set status of order failed"
                });
            }

            // Cập nhật trạng thái
            orderExists.Status = (OrderStatus)orderStatus;
            _context.Entry(orderExists).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new BaseResponseWithData<bool>()
                    {
                        Errors = new List<string>() { "An error occured while update status for order." },
                        IsSuccess = false,
                        Message = "Update status for order failed"
                    });
            }
        }

        /// <summary>
        /// This api to get all order info of current user.
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/v1/orders/getmyorders")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponseWithData<List<OrderInfoResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyOrders()
        {
            var result = await _orderService.GetMyOrdersAsync();
            return StatusCode(statusCode: result.statusCode, value: result.result);
        }


        /// <summary>
        /// This api to get all info of orders (Admin)
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/v1/orders/getalls")]
        [Authorize(Roles = RolesConstant.Admin)]
        
        public async Task<IActionResult> GetAlls()
        {
            var result = await _orderService.GetOrdersAsync();
            return StatusCode(statusCode: result.statusCode, value: result.result);
        }

        [HttpPut("/api/v1/orders/cancelmyorder/{orderId}")]
        public async Task<IActionResult> CancelMyOrder(string orderId)
        {
            var myOrder = await _context.OrderCourses
                .Include(od => od.Payments)
                .Where(od => od.Id == orderId
                    && !od.Payments.Any(p => p.PaymentStatus == "0" 
                        && p.OrderCourseId == od.Id
                        && p.ExpireDate <= DateTime.UtcNow)
                    && od.Status != OrderStatus.Success)
                .FirstOrDefaultAsync();
            if (myOrder == null)
            {
                return BadRequest(new BaseResponseWithData<bool>()
                {
                    IsSuccess = false,
                    Errors = new List<string>(){ $"The order with id : '{orderId}' cannot edit the status." },
                    Message = "Update status for Order failed"
                });
            }
            myOrder.Status = OrderStatus.Cancel;
            _context.OrderCourses.Update(myOrder);
            try
            {
                await _context.SaveChangesAsync();
                return Ok(new BaseResponseWithData<bool>()
                {
                    Data = true,
                    Message = "Update status for order successfully",
                    IsSuccess = true
                });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}