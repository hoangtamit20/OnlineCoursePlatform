using System.Net;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;

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
        private readonly IHubContext<NotificationHub> _hubContext;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userManager"></param>
        /// <param name="hubContext"></param>
        public OrderController(
            OnlineCoursePlatformDbContext context, 
            UserManager<AppUser> userManager,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        // GET: api/Order
        /// <summary>
        /// Get list order
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResponseWithData<List<OrderInfoDto>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BaseBadRequestResult), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetOrders()
        {
            if (_context.Orders == null)
            {
                return NotFound(new BaseBadRequestResult() { Errors = new List<string>() { $"Db 'Order' is null!" } });
            }
            return Ok(new BaseResultWithData<List<OrderInfoDto>>()
            {
                Success = true,
                Message = "List orders",
                Data = (await _context.Orders.ToListAsync()).Adapt<List<OrderInfoDto>>()
            });
        }

        /// <summary>
        /// Get order by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/Order/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultWithData<OrderInfoDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BaseBadRequestResult), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetOrder(int id)
        {
            if (_context.Orders == null)
            {
                return NotFound(new BaseBadRequestResult() { Errors = new List<string>() { $"Db 'Order' is null!" } });
            }
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound(new BaseBadRequestResult() { Errors = new List<string>() { $"Order with id : {id} not found!" } });
            }
            return Ok(new BaseResultWithData<OrderInfoDto>()
            {
                Success = true,
                Message = "Get order by id",
                Data = order.Adapt<OrderInfoDto>()
            });
        }


        /// <summary>
        /// update order by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateOrderDto"></param>
        /// <returns></returns>
        // PUT: api/Order/5
        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(BaseBadRequestResult), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(BaseBadRequestResult), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(BaseBadRequestResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> PutOrder(int id, UpdateOrderDto updateOrderDto)
        {
            if (id != updateOrderDto.Id)
            {
                return BadRequest(new BaseBadRequestResult() { Errors = new List<string>() { $"Route id : {id} not compare OrderId : {updateOrderDto.Id}" } });
            }

            _context.Entry<Order>(updateOrderDto.Adapt<Order>()).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound(new BaseBadRequestResult() { Errors = new List<string>() { $"Route id : {id} not compare OrderId : {updateOrderDto.Id}" } });
                }
                else
                {
                    return StatusCode(500, new BaseBadRequestResult() { Errors = new List<string>() { "Internal Error Server!" } });
                }
            }

            return NoContent();
        }


        /// <summary>
        /// Create order (Authorize)
        /// </summary>
        /// <param name="createOrderDto">Bao gồm : {Thông tin người order; Danh sách sản phẩm đặt hàng}</param>
        /// <returns></returns>
        // POST: api/Order
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(BaseResultWithData<OrderInfoDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BaseBadRequestResult), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(BaseBadRequestResult), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(BaseBadRequestResult), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(BaseBadRequestResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult> PostOrder([FromBody] CreateOrderDto createOrderDto)
        {
            if (ModelState.IsValid)
            {
                if (createOrderDto.ListProductOrder.IsNullOrEmpty())
                {
                    return BadRequest(new BaseBadRequestResult() { Errors = new List<string>() { $"There are no products to place an order" } });
                }
                if (_context.Orders == null)
                {
                    return NotFound(new BaseBadRequestResult() { Errors = new List<string>() { $"Db 'Order' is null!" } });
                }

                using (var _transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var listError = checkValidQuantityOfProductOrder(createOrderDto.ListProductOrder);
                        if (listError.Count > 0)
                        {
                            return BadRequest(new BaseBadRequestResult() { Errors = listError });
                        }

                        // process create order
                        var order = createOrderDto.Adapt<Order>();
                        var currentUser = await _userManager.GetUserAsync(User);
                        if (currentUser == null)
                        {
                            // Handle the case where the claim is not found
                            return Unauthorized(new BaseBadRequestResult() { Errors = new List<string>() { $"the claim is not found" } });
                        }
                        else
                        {
                            order.UserId = currentUser.Id;
                            _context.Orders.Add(order);
                            order.Status = OrderStatus.InProgress;
                            await _context.SaveChangesAsync();
                        }
                        // add product to order details
                        var listOrderDetail = createOrderDto.ListProductOrder.Adapt<List<OrderDetail>>();
                        decimal totalPrice = 0;
                        foreach (var orderDetail in listOrderDetail)
                        {
                            var product = await _context.Products.FindAsync(orderDetail.ProductId);
                            if (product == null)
                            {
                                // Handle the case where the product does not exist
                                listError.Add($"Product with Id : {orderDetail.ProductId} does not exists");
                                continue;
                            }
                            
                            // substrack stock of product
                            product.Stock -= orderDetail.Quantity;
                            _context.Entry<Product>(product).State = EntityState.Modified;

                            // check product has apply discount
                            var price = product.Price - (await GetPriceProductOrder(product));
                            orderDetail.OrderId = order.Id;
                            totalPrice += price * orderDetail.Quantity;
                            orderDetail.SubTotal = price * orderDetail.Quantity;
                        }

                        order.TotalPrice = totalPrice;
                        _context.Entry<Order>(order).State = EntityState.Modified;
                        await _context.OrderDetails.AddRangeAsync(listOrderDetail);
                        await _context.SaveChangesAsync();
                        _transaction.Commit();

                        return Ok(new BaseResultWithData<OrderInfoDto>
                        {
                            Success = true,
                            Message = "Create Order",
                            Data = order.Adapt<OrderInfoDto>()
                        });
                    }
                    catch (Exception ex)
                    {
                        _transaction.Rollback();
                        return StatusCode(500, new BaseBadRequestResult() { Errors = new List<string>() { $"Internal Server Error - {ex.Message}" } });
                    }
                }
            }
            return BadRequest(new BaseBadRequestResult() { Errors = ModelState.SelectMany(x => x.Value!.Errors.Select(p => p.ErrorMessage)).ToList() });
        }


        /// <summary>
        /// Set status for order by id (Admin)
        /// </summary>
        /// <param name="id">Order Id</param>
        /// <param name="orderStatus">Bao gồm các value : {InProgress; Confirmed; Shipping; Success; Canceled}</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{id}/set-order-status")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(BaseBadRequestResult), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(BaseBadRequestResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SetOrderStatus(int id, string orderStatus)
        {
            var orderExists = await _context.Orders.FindAsync(id);
            if (orderExists is null)
                return NotFound(new BaseBadRequestResult() { Errors = new List<string>() { $"Order with Id : {id} not found" } });

            if (Enum.TryParse<OrderStatus>(orderStatus, true, out var status))
            {
                // Cập nhật trạng thái
                orderExists.Status = status;

                // Lưu thay đổi
                _context.Entry(orderExists).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                if (orderExists.Status == OrderStatus.Confirmed)
                {
                    var orderConfirmed = orderExists.Adapt<OrderConfirmedDto>();
                    var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == orderExists.UserId);
                    orderConfirmed.Name = currentUser?.Name!;
                    orderConfirmed.Email = currentUser?.Email!;
                    await _hubContext.Clients.All.SendAsync(SignalRConstant.ReceiveOrderConfirmed, orderConfirmed);
                }

                return NoContent();
            }
            else
            {
                return BadRequest(new BaseBadRequestResult() { Errors = new List<string>() { $"Invalid status value" } });
            }
        }


        private bool OrderExists(int id)
        {
            return (_context.Orders?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private List<string> checkValidQuantityOfProductOrder(List<OrderDetailItemsDto> listOrder)
        {
            List<string> Errors = new List<string>();
            listOrder.ForEach(or =>
            {
                var product = _context.Products.Find(or.ProductId);
                if (product is null)
                {
                    Errors.Add($"Product with Id : {or.ProductId} does not exists");
                }
                else
                {
                    if (or.Quantity > product.Stock)
                    {
                        var productName = (_context.ProductTranslations.FirstOrDefault(pt => pt.ProductId == product.Id && pt.LanguageId == "VN"))!.Name;
                        if (product.Stock < 1)
                        {
                            Errors.Add($"Sorry, the store is currently out of product {productName}");
                        }
                        else
                        {
                            Errors.Add($"The quantity of product {productName} left is only: {product.Stock}");
                        }
                    }
                }
            });
            return Errors;
        }


        private async Task<decimal> GetPriceProductOrder(Product product)
        {
            var currentDate = DateTime.Now;
            var productIsDiscount = await _context.Products
                .Include(p => p.PromotionProducts).ThenInclude(pp => pp.Promotion)
                .Include(p => p.Category).ThenInclude(c => c.PromotionCategories).ThenInclude(pc => pc.Promotion)
                .Where(p =>
                    (p.PromotionProducts.Any(pp => pp.Promotion!.FromDate <= currentDate && pp.Promotion.ToDate >= currentDate)
                    || p.Category.PromotionCategories.Any(pc => pc.Promotion!.FromDate <= currentDate && pc.Promotion.ToDate >= currentDate))
                    && p.Id == product.Id
                )
                .Select(p => new
                {
                    TotalPriceProductDiscount = p.PromotionProducts
                        .Where(pp => pp.Promotion!.FromDate <= currentDate && pp.Promotion.ToDate >= currentDate)
                        .Select(pp => new
                        {
                            PriceDisounted = pp.Promotion!.DiscountType == DiscountType.Percent 
                            ? ((pp.Promotion.DiscountValue > 1 ? 
                                (pp.Promotion.DiscountValue / 100) 
                                : pp.Promotion.DiscountValue) * p.Price) 
                            : pp.Promotion.DiscountValue
                        })
                        .Union(p.Category.PromotionCategories
                            .Where(pc => pc.Promotion!.FromDate <= currentDate && pc.Promotion.ToDate >= currentDate)
                        .Select(pc => new
                        {
                            PriceDisounted = pc.Promotion!.DiscountType == DiscountType.Percent ? ((pc.Promotion.DiscountValue > 1 ? (pc.Promotion.DiscountValue / 100) : pc.Promotion.DiscountValue) * p.Price) : pc.Promotion.DiscountValue
                        }))
                        .Sum(l => l.PriceDisounted)
                }).FirstOrDefaultAsync();

            return productIsDiscount is null ? 0 : productIsDiscount.TotalPriceProductDiscount;
        }
    }
}