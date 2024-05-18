using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Constants;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.Data.Entities.Order;
using OnlineCoursePlatform.DTOs.OrderDtos;
using OnlineCoursePlatform.Helpers;

namespace OnlineCoursePlatform.Services.OrderServices
{
    public class OrderService : IOrderService
    {
        private readonly OnlineCoursePlatformDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager;
        // private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly ILogger<OrderService> _logger;

        public OrderService(OnlineCoursePlatformDbContext dbContext,
            UserManager<AppUser> userManager,
            IHttpContextAccessor httpAccessor,
            ILogger<OrderService> logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _httpAccessor = httpAccessor;
            _logger = logger;
        }

        public async Task<(int statusCode, BaseResponseWithData<CreateOrderResponseDto> result)>
            CreateOrderAsync(CreateOrderRequestDto requestDto)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return BaseReturnHelper<CreateOrderResponseDto>.GenerateErrorResponse(
                    statusCode: StatusCodes.Status401Unauthorized,
                    errorMessage: $"Invalid authentication",
                    message: "Create order failed",
                    data: null);
            }
            var courses = await _dbContext.Courses
                .Where(course => course.IsPublic
                    && !course.IsFree
                    && requestDto.CourseIds.Contains(course.Id))
                .ToListAsync();
            // add order course
            var orderCourse = new OrderCourse()
            {
                UserId = currentUser.Id,
                TotalPrice = courses.Sum(c => c.Price),
                Status = OrderStatus.Draft
            };
            _dbContext.OrderCourses.Add(orderCourse);
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BaseReturnHelper<CreateOrderResponseDto>.GenerateErrorResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    errorMessage: "An error ocurred while create order",
                    message: "Create order failed",
                    data: null);
            }
            // add order items
            var orderItems = new List<OrderDetail>();
            courses.ForEach(c =>
            {
                orderItems.Add(new OrderDetail()
                {
                    CourseId = c.Id,
                    OrderCourseId = orderCourse.Id,
                    ExpireDate = DateTime.UtcNow.AddDays(c.ExpirationDay),
                    Price = c.Price
                });
            });
            _dbContext.OrderDetails.AddRange(orderItems);
            try
            {
                await _dbContext.SaveChangesAsync();
                return BaseReturnHelper<CreateOrderResponseDto>.GenerateSuccessResponse(
                    data: new CreateOrderResponseDto()
                    {
                        Id = orderCourse.Id,
                        OrderDate = orderCourse.OrderDate,
                        Status = orderCourse.Status,
                        TotalPrice = orderCourse.TotalPrice
                    },
                    message: "Create order successfully");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return BaseReturnHelper<CreateOrderResponseDto>.GenerateErrorResponse(
                    errorMessage: "An error occured while create order",
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "Create order failed",
                    data: null);
            }
        }


        public async Task<(int statusCode, BaseResponseWithData<OrderDetailDto> result)> GetOrderAsync(string orderId)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return BaseReturnHelper<OrderDetailDto>.GenerateErrorResponse(
                    errorMessage: "Invalid authentication",
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Get order failed",
                    data: null);
            }
            var currentOrder = await _dbContext.OrderCourses.FindAsync(orderId);
            if (currentOrder == null)
            {
                return BaseReturnHelper<OrderDetailDto>.GenerateErrorResponse(
                    errorMessage: $"The order with id '{orderId}' not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    message: "Get order failed",
                    data: null);
            }
            var roleOfCurrentUser = await _userManager.GetRolesAsync(currentUser);
            if (currentOrder.UserId != currentUser.Id 
                && !roleOfCurrentUser.IsNullOrEmpty()
                && !roleOfCurrentUser.Contains(RolesConstant.Admin))
            {
                return BaseReturnHelper<OrderDetailDto>.GenerateErrorResponse(
                    errorMessage: $"You don't have permission to access this function.",
                    statusCode: StatusCodes.Status403Forbidden,
                    message: "Get order failed",
                    data: null); 
            }
            var orderItems = await _dbContext.OrderDetails
                .Where(od => od.OrderCourseId == currentOrder.Id)
                .Select(od => new OrderItemDto()
                {
                    CourseId = od.CourseId,
                    Price = od.Price,
                    ExpireDate = od.ExpireDate,
                    OrderDate = od.OrderDate
                }).ToListAsync();
            return BaseReturnHelper<OrderDetailDto>.GenerateSuccessResponse(
                data: new OrderDetailDto()
                {
                    OrdererName = currentOrder.UserId == currentUser.Id ? currentUser.Name : 
                        (await _userManager.FindByIdAsync(currentOrder.UserId))?.Name,
                    OrderId = currentOrder.Id,
                    Status = currentOrder.Status,
                    TotalPrice = currentOrder.TotalPrice,
                    OrderItems = orderItems
                },
                message: "Get order successfully");
        }

        public async Task<(int statusCode, BaseResponseWithData<List<OrderInfoResponseDto>> result)> GetMyOrdersAsync()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return BaseReturnHelper<List<OrderInfoResponseDto>>.GenerateErrorResponse(
                    errorMessage: "Invalid authentication",
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Get my orders failed",
                    data: null);
            }
            var myOrders = await _dbContext.OrderCourses
                .Include(oc => oc.OrderDetails)
                .Where(oc => oc.UserId == currentUser.Id)
                .Select(oc => new OrderInfoResponseDto()
                {
                    OrderId = oc.Id,
                    OrderDate = oc.OrderDate,
                    Status = oc.Status,
                    Price = oc.TotalPrice,
                    Quantity = oc.OrderDetails.Count(c => c.OrderCourseId == oc.Id)
                })
                .ToListAsync();
            return BaseReturnHelper<List<OrderInfoResponseDto>>.GenerateSuccessResponse(
                data: myOrders,
                message: "Get my order successfully");
        }

        public async Task<(int statusCode, BaseResponseWithData<List<OrderInfoForAdminDto>> result)> GetOrdersAsync()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return BaseReturnHelper<List<OrderInfoForAdminDto>>.GenerateErrorResponse(
                    errorMessage: "Invalid authentication",
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Get my orders failed",
                    data: null);
            }
            var orders = await _dbContext.OrderCourses
                .Include(oc => oc.User)
                .Include(oc => oc.OrderDetails)
                .Select(oc => new OrderInfoForAdminDto()
                {
                    OrderId = oc.Id,
                    OrderDate = oc.OrderDate,
                    Price = oc.TotalPrice,
                    Status = oc.Status,
                    Quantity = oc.OrderDetails.Count(od => od.OrderCourseId == oc.Id),
                    OrderOwnerInfoDto = new OrderOwnerInfoDto()
                    {
                        UserId = oc.UserId,
                        OrdererName = oc.User.Name,
                        Picture = oc.User.Picture
                    }
                })
                .ToListAsync();
            return BaseReturnHelper<List<OrderInfoForAdminDto>>.GenerateSuccessResponse(
                data: orders,
                message: "Get orders successfully");
        }

        // public async Task<BaseResponseWithData<bool>> DeleteOrderAsync()
        // {

        // }

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