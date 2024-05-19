using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Constants;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;

namespace OnlineCoursePlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavoriteCourseController : ControllerBase
    {
        private readonly OnlineCoursePlatformDbContext _dbContext;

        private readonly UserManager<AppUser> _userManager;

        private readonly IHttpContextAccessor _httpAcessor;

        private readonly ILogger<OrderController> _logger;

        public FavoriteCourseController(
            OnlineCoursePlatformDbContext dbContext,
            IHttpContextAccessor httpAcessor,
            UserManager<AppUser> userManager,
            ILogger<OrderController> logger)
        {
            _dbContext = dbContext;
            _httpAcessor = httpAcessor;
            _userManager = userManager;
            _logger = logger;
        }


        [HttpGet("/api/v1/favoritecourse/getalls")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponseWithData<List<FavoirteCourseResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyFavoriteCourses()
        {
            var currentUser = await _userManager.GetUserAsync(this.User);
            if (currentUser == null)
            {
                return StatusCode(statusCode: StatusCodes.Status401Unauthorized,
                    value: new BaseResponseWithData<List<FavoirteCourseResponseDto>>()
                    {
                        Data = null,
                        Errors = new List<string>() { $"Invalid Authentication" },
                        IsSuccess = false,
                        Message = "Add favorite course failed."
                    });
            }
            var myFavoriteCourses = await _dbContext.UserFavoriteCourses
                .Include(ufc => ufc.Course)
                .Include(ufc => ufc.Course.User)
                .Where(ufc => ufc.UserId == currentUser.Id)
                .Select(ufc => new FavoirteCourseResponseDto()
                {
                    FavoriteCourseId = ufc.Id,
                    CourseId = ufc.CourseId,
                    CourseName = ufc.Course.Name,
                    Price = ufc.Course.Price,
                    Thumbnail = ufc.Course.Thumbnail,
                    OwnerId = ufc.Course.UserId,
                    OwnerName = ufc.Course.User.Name
                })
                .ToListAsync();
            return Ok(new BaseResponseWithData<List<FavoirteCourseResponseDto>>()
            {
                Data = myFavoriteCourses,
                IsSuccess = true,
                Message = "Get my favorite courses successfully"
            });
        }

        [HttpGet("/api/v1/favoritecourse/{favoriteCourseId}")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponseWithData<FavoirteCourseResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyFavoriteCourseById(string favoriteCourseId)
        {
            var currentUser = await _userManager.GetUserAsync(this.User);
            if (currentUser == null)
            {
                return StatusCode(statusCode: StatusCodes.Status401Unauthorized,
                    value: new BaseResponseWithData<FavoirteCourseResponseDto>()
                    {
                        Data = null,
                        Errors = new List<string>() { $"Invalid Authentication" },
                        IsSuccess = false,
                        Message = "Get favorite course failed."
                    });
            }

            var myFavoriteCourse = await _dbContext.UserFavoriteCourses
                .FindAsync(favoriteCourseId);
            if (myFavoriteCourse == null)
            {
                return StatusCode(statusCode: StatusCodes.Status404NotFound,
                    value: new BaseResponseWithData<FavoirteCourseResponseDto>()
                    {
                        Data = null,
                        Errors = new List<string>() { $"Favorite course with id : '{favoriteCourseId}' not found." },
                        IsSuccess = false,
                        Message = "Get favorite course failed."
                    });
            }

            var roles = await _userManager.GetRolesAsync(currentUser);
            if (myFavoriteCourse.UserId != currentUser.Id && !roles.IsNullOrEmpty() && !roles.Contains(RolesConstant.Admin))
            {
                return StatusCode(statusCode: StatusCodes.Status403Forbidden,
                    value: new BaseResponseWithData<FavoirteCourseResponseDto>()
                    {
                        Data = null,
                        Errors = new List<string>() { $"You don't have permission access to this function." },
                        IsSuccess = false,
                        Message = "Get favorite course failed."
                    });
            }

            return Ok(new BaseResponseWithData<FavoirteCourseResponseDto>()
            {
                Data = await _dbContext.UserFavoriteCourses
                    .Where(ufc => ufc.Id == favoriteCourseId)
                    .Select(ufc => new FavoirteCourseResponseDto()
                    {
                        FavoriteCourseId = ufc.Id,
                        CourseId = ufc.CourseId,
                        CourseName = ufc.Course.Name,
                        Price = ufc.Course.Price,
                        Thumbnail = ufc.Course.Thumbnail,
                        OwnerId = ufc.Course.UserId,
                        OwnerName = ufc.Course.User.Name
                    })
                    .FirstOrDefaultAsync(),
                IsSuccess = true,
                Message = "Get favorite course successfully"
            });
        }


        [HttpPost("/api/v1/favoritecourse/create")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponseWithData<UserFavoriteCourse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddFavoriteCourse(FavoriteCourseRequestDto requestDto)
        {
            var currentUser = await _userManager.GetUserAsync(this.User);
            if (currentUser == null)
            {
                return StatusCode(statusCode: StatusCodes.Status401Unauthorized,
                    value: new BaseResponseWithData<UserFavoriteCourse>()
                    {
                        Data = null,
                        Errors = new List<string>() { $"Invalid Authentication" },
                        IsSuccess = false,
                        Message = "Add favorite course failed."
                    });
            }
            var favoriteCourse = await _dbContext.UserFavoriteCourses.FirstOrDefaultAsync(fc =>
                requestDto.CourseId == fc.CourseId && fc.UserId == currentUser.Id);
            if (favoriteCourse != null)
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    value: new BaseResponseWithData<UserFavoriteCourse>()
                    {
                        Data = null,
                        Errors = new List<string>() { $"Course with id : '{requestDto.CourseId}' already exist." },
                        IsSuccess = false,
                        Message = "Add favorite course failed."
                    });
            }
            favoriteCourse = new UserFavoriteCourse()
            {
                CourseId = requestDto.CourseId,
                UserId = currentUser.Id
            };
            _dbContext.UserFavoriteCourses.Add(favoriteCourse);
            var courseInteraction = await _dbContext.UserCourseInteractions.FirstOrDefaultAsync(
                uci => uci.CourseId == requestDto.CourseId && uci.UserId == currentUser.Id);
            if (courseInteraction == null)
            {
                courseInteraction = new UserCourseInteraction()
                {
                    CourseId = requestDto.CourseId,
                    UserId = currentUser.Id,
                    FavoriteScore = 1
                };
                _dbContext.UserCourseInteractions.Add(courseInteraction);
            }
            else
            {
                courseInteraction.FavoriteScore += 1;
                _dbContext.UserCourseInteractions.Update(courseInteraction);
            }

            try
            {
                await _dbContext.SaveChangesAsync();
                return Ok(new BaseResponseWithData<UserFavoriteCourse>()
                {
                    Data = new UserFavoriteCourse()
                    {
                        Id = favoriteCourse.Id,
                        CourseId = favoriteCourse.CourseId,
                        DateAdd = favoriteCourse.DateAdd,
                        UserId = favoriteCourse.UserId

                    },
                    IsSuccess = true,
                    Message = "Create favorite course successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    value: new BaseResponseWithData<UserFavoriteCourse>()
                    {
                        Data = null,
                        Errors = new List<string>() { $"An error occured while create favorite course." },
                        IsSuccess = false,
                        Message = "Create favorite course failed"
                    });
            }
        }


        [HttpDelete("/api/v1/favoritecourse/removefavoritecourses")]
        [Authorize]
        public async Task<IActionResult> DeleteFavoriteCourses(DeleteFavoriteCourseDto requestDto)
        {
            var currentUser = await _userManager.GetUserAsync(this.User);
            if (currentUser == null)
            {
                return StatusCode(statusCode: StatusCodes.Status401Unauthorized,
                    value: new BaseResponseWithData<FavoirteCourseResponseDto>()
                    {
                        Data = null,
                        Errors = new List<string>() { $"Invalid Authentication" },
                        IsSuccess = false,
                        Message = "Get favorite course failed."
                    });
            }
            var roles = await _userManager.GetRolesAsync(currentUser);
            var favoriteCourses = await _dbContext.UserFavoriteCourses
                .Where(ufc => requestDto.FavoriteCourseIds.Contains(ufc.Id))
                .ToListAsync();
            var messages = new List<string>();
            var myFavoriteCourses = new List<UserFavoriteCourse>();
            if (!roles.IsNullOrEmpty() && roles.Contains(RolesConstant.Admin))
            {
                myFavoriteCourses = favoriteCourses;
            }
            else
            {
                favoriteCourses.ForEach(fc =>
                {
                    if (fc.UserId != currentUser.Id)
                    {
                        messages.Add($"{fc.Id}");
                    }
                    else
                    {
                        myFavoriteCourses.Add(fc);
                    }
                });
            }

            if (messages.Count > 0)
            {
                messages.Insert(index: 0, item: (myFavoriteCourses.Count > 0 ? $"{myFavoriteCourses.Count} items has been remove successfully." : string.Empty) + $"You don't permission to remove favorite courses with id below : ");
            }

            // remove favorite course and update course interactions
            var interactionCourses = new List<UserCourseInteraction>();
            myFavoriteCourses.ForEach(fc => {
                var courseInteraction = _dbContext.UserCourseInteractions.FirstOrDefault(uci => uci.CourseId == fc.CourseId
                    && uci.UserId == fc.UserId);
                if (courseInteraction != null)
                {
                    courseInteraction.FavoriteScore -= 1;
                    interactionCourses.Add(courseInteraction);
                }
            });
            _dbContext.UserFavoriteCourses.RemoveRange(entities: myFavoriteCourses);
            _dbContext.UserCourseInteractions.UpdateRange(entities: interactionCourses);
            try
            {
                await _dbContext.SaveChangesAsync();
                return Ok(new BaseResponseWithData<bool>()
                {
                    Data = true,
                    IsSuccess = true,
                    Message = messages.Count > 0 ? string.Join(Environment.NewLine, messages) 
                        : $"{myFavoriteCourses.Count} items have been remove successfully."
                });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    value: new BaseResponseWithData<bool>()
                    {
                        IsSuccess = false,
                        Errors = new List<string>(){ $"An error courred while remove favorite course." },
                        Message = "Remove favorite course failed"
                    });
            }
        }
    }


    public class FavoriteCourseRequestDto
    {
        public int CourseId { get; set; }
    }

    public class FavoirteCourseResponseDto : MyCartItemDto
    {
        public string FavoriteCourseId { get; set; } = null!;
    }

    public class DeleteFavoriteCourseDto
    {
        public List<string> FavoriteCourseIds { get; set; } = new();
    }
}