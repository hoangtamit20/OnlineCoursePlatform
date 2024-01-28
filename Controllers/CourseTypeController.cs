using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Constants;
using OnlineCoursePlatform.DTOs.CourseTypeDtos.Request;
using OnlineCoursePlatform.DTOs.CourseTypeDtos.Response;
using OnlineCoursePlatform.Models.CourseTypeModels;
using OnlineCoursePlatform.Models.PagingAndFilter.Filter.CourseTypes;
using OnlineCoursePlatform.Services.CourseTypeServices.Interfaces;

namespace OnlineCoursePlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseTypeController : ControllerBase
    {
        private readonly ICourseTypeService _courseTypeService;

        public CourseTypeController(ICourseTypeService courseTypeService)
        {
            _courseTypeService = courseTypeService;
        }

        /// <summary>
        /// This API is designed to filter the results was paged that have been filtered in the first instance.
        /// </summary>
        /// <param name="courseTypeFilterParams">Includes the details of the pagenumber, pagesize to be filtered.</param>
        /// <returns>Returns the filtered list of CourseTypes if the request is valid or an error message if the CourseTypes could not be filtered.</returns>
        /// <response code="200">Returns the filtered list of CourseTypes.</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/v1/coursetype/getalls-second-filter
        ///     {
        ///        "FirstFilter": "First filter value",
        ///        "SecondFilter": "Second filter value"
        ///     }
        ///
        /// </remarks>
        [HttpGet("/api/v1/coursetype/getalls")]
        [ProducesResponseType(typeof(BaseResponseWithData<BasePagedResultDto<CourseTypeInfoModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCourseTypeAsync([FromQuery] CourseTypeFilterParams courseTypeFilterParams)
        {
            var listResult = await _courseTypeService.GetAllsCoursesTypeServiceAsync(courseTypeFilterParams: courseTypeFilterParams);
            var result = new BasePagedResultDto<CourseTypeInfoModel>()
            {
                CurrentPage = listResult.CurrentPage,
                TotalPages = listResult.TotalPages,
                PageSize = listResult.PageSize,
                TotalItems = listResult.TotalCount,
                HasPrevious = listResult.HasPrevious,
                HasNext = listResult.HasNext,
                FirstFilter = courseTypeFilterParams.Name,
                Items = listResult,
            };
            return Ok(new BaseResponseWithData<BasePagedResultDto<CourseTypeInfoModel>>()
            {
                Data = result,
                IsSuccess = true,
                Message = "List course types"
            });
        }

        /// <summary>
        /// This API is designed to filter and page for the list course type.
        /// </summary>
        /// <param name="courseTypeSecondFilterParams">Includes the details of the pagenumber, pagesize firstfilter, second filter.</param>
        /// <returns>Returns the filtered list of CourseTypes if the request is valid or an error message if the CourseTypes could not be filtered.</returns>
        /// <response code="200">Returns the filtered list of CourseTypes.</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/v1/coursetype/getalls-second-filter
        ///     {
        ///        "FirstFilter": "First filter value",
        ///        "SecondFilter": "Second filter value"
        ///     }
        ///
        /// </remarks>
        [HttpGet("/api/v1/coursetype/getalls-second-filter")]
        [ProducesResponseType(typeof(BaseResponseWithData<BasePagedSecondFilterResultDto<CourseTypeInfoModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCourseTypeAsync([FromQuery] CourseTypeSecondFilterParams courseTypeSecondFilterParams)
        {
            var listResult = await _courseTypeService.GetAllsSecondFilterCourseTypesServiceAsync(
                courseTypeSecondFilterParams: courseTypeSecondFilterParams);
            var result = new BasePagedSecondFilterResultDto<CourseTypeInfoModel>()
            {
                CurrentPage = listResult.CurrentPage,
                TotalPages = listResult.TotalPages,
                PageSize = listResult.PageSize,
                TotalItems = listResult.TotalCount,
                HasPrevious = listResult.HasPrevious,
                HasNext = listResult.HasNext,
                FirstFilter = courseTypeSecondFilterParams.FirstFilter,
                SecondFilter = courseTypeSecondFilterParams.SecondFilter,
                Items = listResult,
            };
            return Ok(new BaseResponseWithData<BasePagedSecondFilterResultDto<CourseTypeInfoModel>>()
            {
                Data = result,
                IsSuccess = true,
                Message = "List course types",
            });
        }



        /// <summary>
        /// This API is designed to find a CourseType by its ID.
        /// </summary>
        /// <param name="id">The ID of the CourseType to be found.</param>
        /// <returns>Returns the CourseType if found, or an error message if the CourseType could not be found.</returns>
        /// <response code="200">Returns the CourseType if found.</response>
        /// <response code="404">If the CourseType could not be found.</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/v1/coursetype/get-course-type/{id}
        ///
        /// </remarks>
        [HttpGet("/api/v1/coursetype/get-course-type/{id}")]
        [ProducesResponseType(typeof(BaseResponseWithData<CourseTypeInfoModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponseWithData<CourseTypeInfoModel>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCourseTypeByIdAsync(int id)
        {
            var (statusCode, result) = await _courseTypeService
                .FindCourseTypeByIdServiceAsync(idCourseType: id);
            return StatusCode(statusCode: statusCode, value: result);
        }

        /// <summary>
        /// This API creates a new CourseType.
        /// </summary>
        /// <param name="createCourseTypeRequestDto">Includes the details of the CourseType to be created.</param>
        /// <returns>Returns the newly created CourseType if the request is valid or an error message if the CourseType could not be created.</returns>
        /// <response code="200">Returns the newly created CourseType.</response>
        /// <response code="400">If the data is not valid.</response>
        /// <response code="409">If a CourseType with the same name already exists.</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/v1/coursetype/create
        ///     {
        ///        "name": "New CourseType Name"
        ///     }
        ///
        /// </remarks>
        [HttpPost("/api/v1/coursetype/create-course-type")]
        [ProducesResponseType(typeof(BaseResponseWithData<CreateCourseTypeResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponseWithData<CreateCourseTypeResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponseWithData<CreateCourseTypeResponseDto>), StatusCodes.Status409Conflict)]
        [Authorize]
        public async Task<IActionResult> CreateCourseTypeAsync(CreateCourseTypeRequestDto createCourseTypeRequestDto)
        {
            if (ModelState.IsValid)
            {
                var (statusCode, result) = await _courseTypeService
                .CreateCourseTypeServiceAsync(createCourseTypeRequestDto: createCourseTypeRequestDto);
                return StatusCode(statusCode: statusCode, value: result);
            }
            return BadRequest(new BaseResponseWithData<UpdateCourseTypeResponseDto>()
            {
                IsSuccess = false,
                Message = "Invalid data",
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            });
        }


        /// <summary>
        /// This API updates an existing CourseType.
        /// </summary>
        /// <param name="updateCourseTypeRequestDto">Includes the details of the CourseType to be updated.</param>
        /// <param name="id">Id of course type.</param>
        /// <returns>Returns the updated CourseType if the request is valid or an error message if the CourseType could not be updated.</returns>
        /// <response code="200">Returns the updated CourseType.</response>
        /// <response code="401">Unauthorize.</response>
        /// <response code="403">Forbiden.</response>
        /// <response code="400">If the data is not valid.</response>
        /// <response code="404">If the CourseType is not found.</response>
        /// <response code="409">If a CourseType with the same name already exists.</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /api/v1/coursetype/update
        ///     {
        ///        "id": 1,
        ///        "name": "Updated CourseType Name",
        ///        // ... other properties ...
        ///     }
        ///
        /// </remarks>
        [HttpPut("/api/v1/coursetype/update-course-type/{id}")]
        [ProducesResponseType(typeof(BaseResponseWithData<UpdateCourseTypeResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponseWithData<UpdateCourseTypeResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponseWithData<UpdateCourseTypeResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponseWithData<UpdateCourseTypeResponseDto>), StatusCodes.Status409Conflict)]
        [Authorize(Roles = $"{RolesConstant.Manager}, {RolesConstant.Admin}")]
        public async Task<IActionResult> UpdateCourseTypeAsync(
            int id, [FromBody] UpdateCourseTypeRequestDto updateCourseTypeRequestDto)
        {
            // If data is not valid
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseResponseWithData<UpdateCourseTypeResponseDto>()
                {
                    IsSuccess = false,
                    Message = "Invalid data",
                    Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
                });
            }
            // Check if the ID in the URL matches the ID of the CourseType to be updated
            if (id != updateCourseTypeRequestDto.Id)
            {
                return BadRequest(new BaseResponseWithData<UpdateCourseTypeResponseDto>()
                {
                    Data = null,
                    IsSuccess = false,
                    Errors = new List<string>() { $"The ID in the URL does not match the ID of the CourseType to be updated." },
                    Message = "Update CourseType failed"
                });
            }
            var (statusCode, result) = await _courseTypeService.UpdateCourseTypeServiceAsync(
                updateCourseTypeRequestDto: updateCourseTypeRequestDto);
            return StatusCode(statusCode: statusCode, value: result);
        }

        /// <summary>
        /// This API deletes an existing CourseType.
        /// </summary>
        /// <param name="id">The ID of the CourseType to be deleted.</param>
        /// <returns>Returns a success message if the CourseType is deleted successfully, or an error message if the CourseType could not be deleted.</returns>
        /// <response code="200">Returns a success message if the CourseType is deleted successfully.</response>
        /// <response code="401">Unauthorize</response>
        /// <response code="403">Forbiden</response>
        /// <response code="404">If the CourseType is not found.</response>
        /// <response code="409">If the CourseType is being referenced by other resources.</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /api/v1/coursetype/delete-course-type/{id}
        ///
        /// </remarks>
        [HttpDelete("/api/v1/coursetype/delete-course-type/{id}")]
        [ProducesResponseType(typeof(BaseResponseWithData<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponseWithData<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponseWithData<string>), StatusCodes.Status409Conflict)]
        [Authorize(Roles = $"{RolesConstant.Manager}, {RolesConstant.Admin}")]
        public async Task<IActionResult> DeleteCourseTypeByIdAsync(int id)
        {
            var (statusCode, result) = await _courseTypeService
                .DeleteCourseTypeByIdServiceAsync(idCourseType: id);
            return StatusCode(statusCode: statusCode, value: result);
        }
    }
}