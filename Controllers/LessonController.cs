using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Constants;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.DTOs.LessonDtos;
using OnlineCoursePlatform.Services.LessonServices;

namespace OnlineCoursePlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LessonController : ControllerBase
    {
        private readonly OnlineCoursePlatformDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<LessonController> _logger;
        private readonly ILessonService _lessonService;

        public LessonController(
            OnlineCoursePlatformDbContext dbContext,
            UserManager<AppUser> userManager,
            ILogger<LessonController> logger,
            ILessonService lessonService)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _logger = logger;
            _lessonService = lessonService;
            
        }

        [HttpPost("/api/v1/lessons/lessonsofcourse")]
        [Authorize]
        public async Task<IActionResult> GetLessonsOfCourse([FromBody] GetLessonsOfCourseRequestDto requestDto)
        {
            var result = await _lessonService.GetLessonsOfCourseAsync(requestDto: requestDto);
            return StatusCode(statusCode: result.statusCode, result.data);
        }

        [HttpPost("/api/v1/lesson/addlesson")]
        [Authorize(Roles = $"{RolesConstant.Publisher}")]
        [ProducesResponseType(typeof(AddLessResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddLesson(AddLessonRequestDto requestDto)
        {
            var result = await _lessonService.AddLessonAsync(requestDto);
            return StatusCode(statusCode: result.statusCode, value: result.result);
        }
        
        [HttpGet("/api/v1/lessons/{lessonId}")]
        [Authorize]
        public async Task<IActionResult> GetLessonById(int lessonId)
        {
            var requestDto = new GetLessonDetailRequestDto(){ LessonId = lessonId };
            var result = await _lessonService.GetLessonDetailAsync(requestDto);
            return StatusCode(statusCode: result.statusCode, value: result.result);
        }






        
    }
}