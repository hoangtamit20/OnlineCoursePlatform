using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Constants;
using OnlineCoursePlatform.DTOs.CourseTopicDtos.Request;
using OnlineCoursePlatform.DTOs.CourseTopicDtos.Response;
using OnlineCoursePlatform.DTOs.CourseTypeDtos.Response;
using OnlineCoursePlatform.Models.CourseTopicModels;
using OnlineCoursePlatform.Models.PagingAndFilter.Filter.CourseTopics;
using OnlineCoursePlatform.Services.CourseTopicServices.Interfaces;

namespace OnlineCoursePlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseTopicController : ControllerBase
    {
        private readonly ICourseTopicService _courseTopicService;

        public CourseTopicController(ICourseTopicService courseTopicService)
        {
            _courseTopicService = courseTopicService;
        }

        /// <summary>
        /// This API is designed to filter the results was paged that have been filtered in the first instance.
        /// </summary>
        /// <param name="courseTopicFilterParams">Includes the details of the pagenumber, pagesize to be filtered.</param>
        /// <returns>Returns the filtered list of CourseTopics if the request is valid or an error message if the CourseTopics could not be filtered.</returns>
        /// <response code="200">Returns the filtered list of CourseTopics.</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/v1/coursetopic/getalls-second-filter
        ///     {
        ///        "FirstFilter": "First filter value",
        ///        "SecondFilter": "Second filter value"
        ///     }
        ///
        /// </remarks>
        [HttpGet("/api/v1/coursetopic/getalls")]
        [ProducesResponseType(typeof(BaseResponseWithData<BasePagedResultDto<CourseTopicInfoModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCourseTypeAsync([FromQuery] CourseTopicFilterParams courseTopicFilterParams)
        {
            var listResult = await _courseTopicService.GetAllsCoursesTopicsServiceAsync(courseTopicFilterParams: courseTopicFilterParams);
            var result = new BasePagedResultDto<CourseTopicInfoModel>()
            {
                CurrentPage = listResult.CurrentPage,
                TotalPages = listResult.TotalPages,
                PageSize = listResult.PageSize,
                TotalItems = listResult.TotalCount,
                HasPrevious = listResult.HasPrevious,
                HasNext = listResult.HasNext,
                FirstFilter = courseTopicFilterParams.Name,
                Items = listResult,
            };
            return Ok(new BaseResponseWithData<BasePagedResultDto<CourseTopicInfoModel>>()
            {
                Data = result,
                IsSuccess = true,
                Message = "List course topics"
            });
        }

        /// <summary>
        /// This API is designed to filter and page for the list course topic.
        /// </summary>
        /// <param name="courseTopicSecondFilterParams">Includes the details of the pagenumber, pagesize firstfilter, second filter.</param>
        /// <returns>Returns the filtered list of CourseTopics if the request is valid or an error message if the CourseTopics could not be filtered.</returns>
        /// <response code="200">Returns the filtered list of CourseTopics.</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/v1/coursetopic/getalls-second-filter
        ///     {
        ///        "FirstFilter": "First filter value",
        ///        "SecondFilter": "Second filter value"
        ///     }
        ///
        /// </remarks>
        [HttpGet("/api/v1/coursetopic/getalls-second-filter")]
        [ProducesResponseType(typeof(BaseResponseWithData<BasePagedSecondFilterResultDto<CourseTopicInfoModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCourseTypeAsync([FromQuery] CourseTopicSecondFilterParams courseTopicSecondFilterParams)
        {
            var listResult = await _courseTopicService.GetAllsSecondFilterCourseTopicsServiceAsync(
                courseTopicSecondFilterParams: courseTopicSecondFilterParams);
            var result = new BasePagedSecondFilterResultDto<CourseTopicInfoModel>()
            {
                CurrentPage = listResult.CurrentPage,
                TotalPages = listResult.TotalPages,
                PageSize = listResult.PageSize,
                TotalItems = listResult.TotalCount,
                HasPrevious = listResult.HasPrevious,
                HasNext = listResult.HasNext,
                FirstFilter = courseTopicSecondFilterParams.FirstFilter,
                SecondFilter = courseTopicSecondFilterParams.SecondFilter,
                Items = listResult,
            };
            return Ok(new BaseResponseWithData<BasePagedSecondFilterResultDto<CourseTopicInfoModel>>()
            {
                Data = result,
                IsSuccess = true,
                Message = "List course types",
            });
        }


        /// <summary>
        /// This API is designed to find a CourseTopic by its ID.
        /// </summary>
        /// <param name="id">The ID of the CourseTopic to be found.</param>
        /// <returns>Returns the CourseTopic if found, or an error message if the CourseTopic could not be found.</returns>
        /// <response code="200">Returns the CourseTopic if found.</response>
        /// <response code="404">If the CourseTopic could not be found.</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/v1/coursetopic/get-course-topic/{id}
        ///
        /// </remarks>
        [HttpGet("/api/v1/coursetopic/get-course-topic/{id}")]
        [ProducesResponseType(typeof(BaseResponseWithData<CourseTopicInfoModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponseWithData<CourseTopicInfoModel>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCourseTopicAsync(int id)
        {
            var (statusCode, result) = await _courseTopicService.GetCourseTopicServiceAsync(id);
            return StatusCode(statusCode: statusCode, value: result);
        }

        /// <summary>
        /// This API creates a new CourseTopic.
        /// </summary>
        /// <param name="createCourseTopicRequestDto">Includes the details of the CourseTopic to be created.</param>
        /// <returns>Returns the newly created CourseTopic if the request is valid or an error message if the CourseTopic could not be created.</returns>
        /// <response code="200">Returns the newly created CourseTopic.</response>
        /// <response code="400">If the data is not valid.</response>
        /// <response code="409">If a CourseTopic with the same name already exists.</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/v1/coursetopic/create
        ///     {
        ///        "name": "New CourseTopic Name",
        ///        "courseTypeId" : id
        ///     }
        ///
        /// </remarks>
        [HttpPost("/api/v1/coursetopic/create-course-topic")]
        [ProducesResponseType(typeof(BaseResponseWithData<CreateCourseTopicResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponseWithData<CreateCourseTopicResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponseWithData<CreateCourseTopicResponseDto>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateCourseTopicAsync(CreateCourseTopicRequestDto createCourseTopicRequestDto)
        {
            // If data is not valid
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseResponseWithData<CreateCourseTopicResponseDto>()
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Data is invalid",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(err => err.ErrorMessage)
                        .ToList()
                });
            }
            var (statusCode, result) = await _courseTopicService.CreateCourseTopicServiceAsync(
                createCourseTopicRequestDto: createCourseTopicRequestDto);
            return StatusCode(statusCode: statusCode, value: result);
        }


        /// <summary>
        /// This API updates an existing CourseTopic.
        /// </summary>
        /// <param name="updateCourseTopicRequestDto">Includes the details of the CourseTopic to be updated.</param>
        /// <param name="id">Id of course type.</param>
        /// <returns>Returns the updated CourseTopic if the request is valid or an error message if the CourseTopic could not be updated.</returns>
        /// <response code="200">Returns the updated CourseTopic.</response>
        /// <response code="401">Unauthorize.</response>
        /// <response code="403">Forbiden.</response>
        /// <response code="400">If the data is not valid.</response>
        /// <response code="404">If the CourseTopic is not found.</response>
        /// <response code="409">If a CourseTopic with the same name already exists.</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /api/v1/coursetopic/update-course-topic/{id}
        ///     {
        ///        "id": 1,
        ///        "name": "Updated CourseTopic Name",
        ///        // ... other properties ...
        ///     }
        ///
        /// </remarks>
        [HttpPut("/api/v1/coursetopic/update-course-topic/{id}")]
        [ProducesResponseType(typeof(BaseResponseWithData<UpdateCourseTopicResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponseWithData<UpdateCourseTopicResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponseWithData<UpdateCourseTopicResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponseWithData<UpdateCourseTopicResponseDto>), StatusCodes.Status409Conflict)]
        [Authorize(Roles = $"{RolesConstant.Manager}, {RolesConstant.Admin}")]
        public async Task<IActionResult> UpdateCourseTopicAsync(int id, UpdateCourseTopicRequestDto updateCourseTopicRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseResponseWithData<UpdateCourseTopicResponseDto>()
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Data is invalid",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(err => err.ErrorMessage)
                        .ToList()
                });
            }
            // Check if the ID in the URL matches the ID of the CourseType to be updated
            if (id != updateCourseTopicRequestDto.Id)
            {
                return BadRequest(new BaseResponseWithData<UpdateCourseTypeResponseDto>()
                {
                    Data = null,
                    IsSuccess = false,
                    Errors = new List<string>() { $"The ID in the URL does not match the ID of the CourseTopic to be updated." },
                    Message = "Update CourseTopic failed"
                });
            }
            var (statusCode, result) = await _courseTopicService.UpdateCourseTopicServiceAsync(
                updateCourseTopicRequestDto: updateCourseTopicRequestDto);
            return StatusCode(statusCode: statusCode, value: result);
        }


        /// <summary>
        /// This API deletes an existing CourseTopic.
        /// </summary>
        /// <param name="id">The ID of the CourseTopic to be deleted.</param>
        /// <returns>Returns a success message if the CourseTopic is deleted successfully, or an error message if the CourseTopic could not be deleted.</returns>
        /// <response code="200">Returns a success message if the CourseTopic is deleted successfully.</response>
        /// <response code="401">Unauthorize</response>
        /// <response code="403">Forbiden</response>
        /// <response code="404">If the CourseTopic is not found.</response>
        /// <response code="409">If the CourseTopic is being referenced by other resources.</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /api/v1/coursetopic/delete-course-type/{id}
        ///
        /// </remarks>
        [HttpDelete("/api/v1/coursetopic/delete-course-topic/{id}")]
        public async Task<IActionResult> DeleteCourseTopicAsync(int id)
        {
            var (statusCode, result) = await _courseTopicService.DeleteCourseTopicAsync(id);
            return StatusCode(statusCode: statusCode, value: result);
        }
    }
}