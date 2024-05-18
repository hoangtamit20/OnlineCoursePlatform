using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.Data.Entities.CartCollection;
using OnlineCoursePlatform.DTOs.CartDtos;

namespace OnlineCoursePlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly OnlineCoursePlatformDbContext _dbContext;
        private readonly ILogger<CartController> _logger;
        private readonly UserManager<AppUser> _userManager;

        public CartController(OnlineCoursePlatformDbContext dbContext,
            ILogger<CartController> logger,
            UserManager<AppUser> userManager)
        {
            _dbContext = dbContext;
            _logger = logger;
            _userManager = userManager;
        }


        [HttpGet("/api/v1/carts/getall")]
        [Authorize]
        public async Task<IActionResult> GetCarts()
        {
            var currentUser = await _userManager.GetUserAsync(this.User);
            if (currentUser == null)
            {
                return Unauthorized();
            }
            var myCart = await _dbContext.Carts.FirstOrDefaultAsync(c => c.UserId == currentUser.Id);
            if (myCart == null)
            {
                myCart = new Cart()
                {
                    UserId = currentUser.Id
                };
                _dbContext.Carts.Add(myCart);
                try
                {
                    await _dbContext.SaveChangesAsync();
                    return Ok(new BaseResponseWithData<CartResponseDto>()
                    {
                        Data = new CartResponseDto()
                        {
                            OwnerName = currentUser.Name,
                            ToltalPrice = 0
                        },
                        IsSuccess = true,
                        Message = "Get cart successfully"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseWithData<CartResponseDto>()
                    {
                        IsSuccess = false,
                        Data = null,
                        Errors = new List<string>() { $"An error occured while get carts" },
                        Message = "Get cart failed"
                    });
                }
            }
            var cartItems = await _dbContext.CartItems
                .Include(ct => ct.Course)
                .Where(ct => ct.CartId == myCart.Id)
                .Select(ct => new CartItemInfoDto()
                {
                    CourseId = ct.CourseId,
                    CourseName = ct.Course.Name,
                    Price = ct.Course.Price,
                    Thumbnail = ct.Course.Thumbnail,
                    DateAdded = ct.DateAdd
                }).ToListAsync();
            return Ok(new BaseResponseWithData<CartResponseDto>()
            {
                Data = new CartResponseDto()
                {
                    OwnerName = currentUser.Name,
                    ToltalPrice = cartItems.Sum(ct => ct.Price),
                    CartItemInfoDtos = cartItems
                },
                IsSuccess = true,
                Message = "Get cart successfully"
            });
        }

        [HttpPost("/api/v1/carts/add")]
        [Authorize]
        public async Task<IActionResult> AddCart(CartRequestDto requestDto)
        {
            var messages = new List<string>();
            var currentUser = await _userManager.GetUserAsync(this.User);
            if (currentUser == null)
            {
                return Unauthorized();
            }
            var myCart = await _dbContext.Carts.Where(c => c.UserId == currentUser.Id)
                .FirstOrDefaultAsync();
            // if my cart not exit -> create new
            if (myCart == null)
            {
                myCart = new Cart()
                {
                    UserId = currentUser.Id
                };
                _dbContext.Carts.Add(myCart);
                try
                {
                    await _dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    return StatusCode(statusCode: (int)HttpStatusCode.InternalServerError,
                        value: new BaseResponseWithData<AddCartResponseDto>()
                        {
                            Data = null,
                            Errors = new List<string>() { "An error occured while create cart." },
                            IsSuccess = false,
                            Message = "Add course to cart failed"
                        });
                }
            }
            // add course to cart
            var cartItems = new List<CartItem>();
            requestDto.CourseIds.ForEach(id =>
            {
                var courseExist = _dbContext.Courses.Find(id);
                var cartItemExist = _dbContext.CartItems
                        .Where(ct => ct.CartId == myCart.Id && ct.CourseId == id)
                        .FirstOrDefault();
                if (courseExist != null && cartItemExist == null)
                {
                    cartItems.Add(new CartItem()
                    {
                        CartId = myCart.Id,
                        CourseId = id
                    });
                }
                else
                {
                    messages.Add($"Course '{courseExist?.Name}' already exists in the shopping cart.");
                }
            });
            _dbContext.CartItems.AddRange(entities: cartItems);
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError,
                    value: new BaseResponseWithData<AddCartResponseDto>()
                    {
                        Data = null,
                        Errors = new List<string>() { "An error occured while add course to cart." },
                        Message = "Add course to card failed",
                        IsSuccess = false
                    });
            }
            return Ok(new BaseResponseWithData<AddCartResponseDto>()
            {
                IsSuccess = true,
                Message = string.Join(Environment.NewLine, messages),
                Data = new AddCartResponseDto() { QuantityOfItemsAdded = cartItems.Count }
            });
        }

        [HttpDelete("/api/v1/carts/remove")]
        [Authorize]
        public async Task<IActionResult> DeleteCartItems(CartRequestDto requestDto)
        {
            var currentUser = await _userManager.GetUserAsync(this.User);
            if (currentUser == null)
            {
                return Unauthorized();
            }
            var myCart = await _dbContext.Carts.FirstOrDefaultAsync(c => c.UserId == currentUser.Id);
            if (myCart == null)
            {
                return NotFound(new BaseResponseWithData<bool>()
                {
                    IsSuccess = false,
                    Message = $"Delete course in cart failed",
                    Errors = new List<string>() { $"Can not found any cart of user {currentUser.Id}" }
                });
            }
            var cartItems = new List<CartItem>();
            requestDto.CourseIds.ForEach(id =>
            {
                var cartItemExist = _dbContext.CartItems.FirstOrDefault(
                    ct => ct.CourseId == id && ct.CartId == myCart.Id);
                if (cartItemExist != null)
                {
                    cartItems.Add(cartItemExist);
                }
            });
            _dbContext.CartItems.RemoveRange(entities: cartItems);
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError,
                    value: new BaseResponseWithData<bool>()
                    {
                        Errors = new List<string>() { $"An error occured while delete course in cart." },
                        IsSuccess = false,
                        Message = "Delete course in cart failed"
                    });
            }
            return Ok(new BaseResponseWithData<bool>()
            {
                Data = true,
                IsSuccess = true,
                Message = "Delete course in cart successfully"
            });
        }
    }

    public class CartRequestDto
    {
        public List<int> CourseIds { get; set; } = new();
    }

    public class AddCartResponseDto
    {
        public AddCartResponseDto()
        {
        }

        public int QuantityOfItemsAdded { get; set; }
    }
}