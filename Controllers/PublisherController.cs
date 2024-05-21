using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Constants;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.Data.Entities.Order;

namespace OnlineCoursePlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublisherController : ControllerBase
    {
        private readonly OnlineCoursePlatformDbContext _dbContext;
        private readonly ILogger<PublisherController> _logger;
        private readonly UserManager<AppUser> _userManager;

        public PublisherController(
            OnlineCoursePlatformDbContext dbContext,
            ILogger<PublisherController> logger,
            UserManager<AppUser> userManager)
        {
            _dbContext = dbContext;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpPost("/api/v1/publisher/create")]
        [Authorize]
        public async Task<IActionResult> BecomeToPublisher()
        {
            var currentUser = await _userManager.GetUserAsync(this.User);
            if (currentUser == null)
            {
                return Unauthorized();
            }
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (!roles.IsNullOrEmpty() && !roles.Any(r => r.Contains(RolesConstant.Publisher)))
            {
                try
                {
                    await _userManager.AddToRoleAsync(user: currentUser, role: RolesConstant.Publisher);
                    return Ok(new BaseResponseWithData<bool>()
                    {
                        IsSuccess = true,
                        Message = "Create publisher successfully",
                        Data = true
                    });
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex.Message);
                    return StatusCode(statusCode: StatusCodes.Status500InternalServerError, value: new BaseResponseWithData<bool>()
                    {
                        Errors = new List<string>(){ $"An error occured while create publisher" },
                        IsSuccess = false,
                        Message = "Create publisher failed"
                    });
                }
            }
            return BadRequest(new BaseResponseWithData<bool>()
            {
                Errors = new List<string>(){ $"You already is a publisher" },
                IsSuccess = false,
                Message = "Create publisher failed"
            });
        }


        [HttpGet("/api/v1/publisher/myprofile")]
        [Authorize(Roles = RolesConstant.Publisher)]
        public async Task<IActionResult> GetMyProfile()
        {
            var currentUser = await _userManager.GetUserAsync(this.User);
            if (currentUser == null)
            {
                return Unauthorized();
            }
            var userInfo = new BaseUserInfoDto()
            {
                UserId = currentUser.Id,
                Name = currentUser.Name,
                DateOfBirth = currentUser.DateOfBirth,
                Address = currentUser.Address,
                Picture = currentUser.Picture,
                DateCreate = currentUser.CreateDate
            };

            var myCourses = await _dbContext.Courses.Where(c => c.UserId == currentUser.Id)
                .Select(c => new CourseUploadedDto()
                {
                    CourseId = c.Id,
                    CourseName = c.Name,
                    IsPublic = c.IsPublic,
                    Price = c.Price,
                    Thumbnail = c.Thumbnail,
                    DateCreate = c.CreateDate
                })
                .ToListAsync();

            var myCourseOrdereds = await _dbContext.OrderCourses.Where(oc => oc.UserId == currentUser.Id)
                .Select(oc => new CourseOrderedDto()
                {
                    OrderCourseId = oc.Id,
                    TotalPrice = oc.TotalPrice,
                    Status = oc.Status,
                    OrderDate = oc.OrderDate
                }).ToListAsync();

            return Ok(new BaseResponseWithData<PublisherProfileResponseDto>()
            {
                Data = new PublisherProfileResponseDto()
                {
                    UserInfo = userInfo,
                    CourseUploadeds = myCourses,
                    CourseOrdereds = myCourseOrdereds
                },
                IsSuccess = true,
                Message = "Get profile successfully",
            });
        }
    }


    public class PublisherProfileResponseDto
    {
        public BaseUserInfoDto UserInfo { get; set; } = null!;
        public List<CourseUploadedDto> CourseUploadeds { get; set; } = new();
        public List<CourseOrderedDto> CourseOrdereds { get; set; } = new();
    }


    public class BaseUserInfoDto
    {
        public string UserId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Picture { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public DateTime DateCreate { get; set; }
    }

    public class CourseUploadedDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = null!;
        public string? Thumbnail { get; set; } = null!;
        public decimal Price { get; set; }
        public bool IsPublic { get; set; }
        public DateTime DateCreate { get; set; }
    }

    public class CourseOrderedDto
    {
        public string OrderCourseId { get; set; } = null!;
        public OrderStatus Status { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
    }
}