using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Constants;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.DTOs.CourseDtos.Request;
using OnlineCoursePlatform.DTOs.CourseDtos.Response;
using OnlineCoursePlatform.Hubs;
using OnlineCoursePlatform.Models.CourseModels;
using OnlineCoursePlatform.Models.PagingAndFilter.Filter.Course;
using OnlineCoursePlatform.Services.CourseServices.Interfaces;

namespace OnlineCoursePlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly IHubContext<ProgressHub> _hubContext;
        private readonly OnlineCoursePlatformDbContext _dbContext;

        private readonly IHttpContextAccessor _httpAcessor;
        public CourseController(
            ICourseService courseService,
            IHubContext<ProgressHub> hubContext,
            OnlineCoursePlatformDbContext dbContext,
            IHttpContextAccessor httpAcessor
            // IAzureMediaService azureMediaService
            )
        {
            _courseService = courseService;
            _hubContext = hubContext;
            _dbContext = dbContext;
            _httpAcessor = httpAcessor;
            // _azureMediaService = azureMediaService;
        }

        [HttpGet("/api/v1/course/get-alls")]
        [ProducesResponseType(typeof(BaseResponseWithData<BasePagedResultDto<CourseInfoModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllsAsync([FromQuery] CourseFilterParams courseFilterParams)
        {
            var listResult = await _courseService.GetAllsCourseServiceAsync(courseFilterParams: courseFilterParams);
            var result = new BasePagedResultDto<CourseInfoModel>()
            {
                CurrentPage = listResult.CurrentPage,
                TotalPages = listResult.TotalPages,
                PageSize = listResult.PageSize,
                TotalItems = listResult.TotalCount,
                HasPrevious = listResult.HasPrevious,
                HasNext = listResult.HasNext,
                FirstFilter = courseFilterParams.Query,
                CourseFilterProperties = courseFilterParams.CourseFilterProperties,
                Items = listResult,
            };
            return Ok(new BaseResponseWithData<BasePagedResultDto<CourseInfoModel>>()
            {
                Data = result,
                IsSuccess = true,
                Message = "List courses"
            });
        }

        [HttpGet("/api/v1/course/getalls-second-filter")]
        [ProducesResponseType(typeof(BaseResponseWithData<BasePagedSecondFilterResultDto<CourseInfoModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCourseTypeAsync([FromQuery] CourseSecondFilterParams courseSecondFilterParams)
        {
            var listResult = await _courseService.GetAllsCourseSecondFilterServiceAsync(
                courseSecondFilterParams: courseSecondFilterParams);
            var result = new BasePagedSecondFilterResultDto<CourseInfoModel>()
            {
                CurrentPage = listResult.CurrentPage,
                TotalPages = listResult.TotalPages,
                PageSize = listResult.PageSize,
                TotalItems = listResult.TotalCount,
                HasPrevious = listResult.HasPrevious,
                HasNext = listResult.HasNext,
                FirstFilter = courseSecondFilterParams.Query,
                SecondFilter = courseSecondFilterParams.SecondQuery,
                CourseFilterProperties = courseSecondFilterParams.CourseFilterProperties,
                Items = listResult,
            };
            return Ok(new BaseResponseWithData<BasePagedSecondFilterResultDto<CourseInfoModel>>()
            {
                Data = result,
                IsSuccess = true,
                Message = "List course",
            });
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="createCourseRequestDto"></param>
        /// <returns></returns>
        [HttpPost("/api/v1/course/create-course")]
        [RequestSizeLimit(bytes: 10485760)] // limit size is 10MB
        [Authorize(Roles = $"{RolesConstant.Publisher}")]
        [ProducesResponseType(type: typeof(BaseResponseWithData<CreateCourseResponseDto>), statusCode: StatusCodes.Status200OK)]
        [ProducesResponseType(type: typeof(BaseResponseWithData<CreateCourseResponseDto>), statusCode: StatusCodes.Status400BadRequest)]
        [ProducesResponseType(type: typeof(BaseResponseWithData<CreateCourseResponseDto>), statusCode: StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCourse(CreateCourseRequestDto createCourseRequestDto)
        {
            var connectionId = Request.Headers["Connection-Id"].ToString();
            await _hubContext.Clients.Client(connectionId: connectionId)
                .SendAsync(method: HubConstants.ReceiveProgress, arg1: "Starting create course .....");
            if (ModelState.IsValid)
            {
                var (statusCode, result) = await _courseService.CreateCourseServiceAsync(
                    createCourseRequestDto: createCourseRequestDto);
                return StatusCode(statusCode: statusCode, value: result);
            }
            // If data is not valid
            return BadRequest(new BaseResponseWithData<CreateCourseResponseDto>()
            {
                IsSuccess = false,
                Message = "Invalid data",
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            });
        }

        [HttpGet("/api/v1/course/get-course/{id}")]
        public async Task<IActionResult> GetCourseDetail(int id)
        {
            var (statusCode, result) = await _courseService.GetCourseDetailServiceAsync(courseId: id);
            return StatusCode(statusCode: statusCode, value: result);
        }

        [HttpGet("/api/v1/course/get-courses-recommend")]
        [Authorize]
        public async Task<IActionResult> GetCoursesWithReccomendation()
        {
            var userId = _httpAcessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ipAddress = _httpAcessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            return Ok(await RecommendCourses(userId: userId, ipAddress: ipAddress, topN: 10));
        }


        private async Task<List<CourseInfoModel>> RecommendCourses(string? userId, string? ipAddress, int topN)
        {
            if (!string.IsNullOrEmpty(userId) || !string.IsNullOrEmpty(ipAddress))
            {
                var similarUsers = await _dbContext.UserCourseInteractions
                    .Where(i => userId != null ? i.UserId != userId : i.IpAddress != ipAddress)
                    .GroupBy(i => userId != null ? i.UserId : i.IpAddress)
                    .Select(g => new
                    {
                        UserId = userId != null ? g.Key : null,
                        IpAddress = userId != null ? null : g.Key,
                        Similarity = g.Sum(i => i.ViewScore + i.PurchaseScore + i.FavoriteScore + i.CommentScore)
                    })
                    .OrderByDescending(u => u.Similarity)
                    .Take(topN)
                    .ToListAsync();

                var recommendedCourseIds = new List<int>();
                foreach (var user in similarUsers)
                {
                    var courseIds = await _dbContext.UserCourseInteractions
                        .Where(i => userId != null ? i.UserId == user.UserId : i.IpAddress == user.IpAddress)
                        .Select(i => i.CourseId)
                        .ToListAsync();
                    recommendedCourseIds.AddRange(courseIds);
                }

                if (recommendedCourseIds.Count < topN)
                {
                    var popularCourseIds = await _dbContext.Courses
                        .Where(c => !_dbContext.UserCourseInteractions
                            .Any(i => i.CourseId == c.Id
                                && (userId != null ? i.UserId == userId : i.IpAddress == ipAddress)))
                        .OrderByDescending(c => c.UserCourseInteractions
                            .Sum(i => i.ViewScore + i.PurchaseScore + i.FavoriteScore + i.CommentScore))
                        .Take(topN - recommendedCourseIds.Count)
                        .Select(c => c.Id)
                        .ToListAsync();

                    recommendedCourseIds.AddRange(popularCourseIds);
                }

                return await _dbContext.Courses.Include(c => c.User)
                    .Where(c => recommendedCourseIds.Contains(c.Id))
                    .OrderByDescending(c => c.UserCourseInteractions
                        .Where(i => i.CourseId == c.Id)
                        .Sum(i => i.ViewScore + i.PurchaseScore + i.FavoriteScore + i.CommentScore))
                    .Select(c => new CourseInfoModel()
                    {
                        Id = c.Id,
                        CourseName = c.Name,
                        Price = c.Price,
                        Thumbnail = c.Thumbnail,
                        IsFree = c.IsFree,
                        WeeklyViews = c.WeeklyViews,
                        MonthlyViews = c.MonthlyViews,
                        CreatorId = c.UserId,
                        CreatorName = c.User.Name,
                        CreatorPicture = c.User.Picture
                    })
                    .ToListAsync();
            }
            else
            {
                // Nếu người dùng chưa đăng nhập, đề xuất topN khóa học phổ biến nhất
                return await _dbContext.Courses
                    .Where(c => _dbContext.UserCourseInteractions.Any(i => i.CourseId == c.Id))
                    .OrderByDescending(c => c.UserCourseInteractions.Sum(i => i.ViewScore + i.PurchaseScore + i.FavoriteScore + i.CommentScore))
                    .Take(topN)
                    .Select(c => new CourseInfoModel()
                    {
                        Id = c.Id,
                        CourseName = c.Name,
                        Price = c.Price,
                        Thumbnail = c.Thumbnail,
                        IsFree = c.IsFree,
                        WeeklyViews = c.WeeklyViews,
                        MonthlyViews = c.MonthlyViews,
                        CreatorId = c.UserId,
                        CreatorName = c.User.Name,
                        CreatorPicture = c.User.Picture
                    })
                    .ToListAsync();
            }
        }
    }
}