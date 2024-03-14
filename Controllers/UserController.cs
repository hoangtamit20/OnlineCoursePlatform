using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.DTOs.UserDtos.Request;
using OnlineCoursePlatform.DTOs.UserDtos.Response;
using OnlineCoursePlatform.Services.UserServices.Interfaces;

namespace OnlineCoursePlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService,
            ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }


        /// <summary>
        /// This method retrieves the basic information of a current user was authorized successfully.
        /// </summary>
        /// <returns>Returns a success message and user data if the user is successfully found, otherwise returns an error message.</returns>
        /// <response code="200">Returns the success message and user data if the user is successfully found</response>
        /// <response code="401">If user is unauthorized</response>
        /// <response code="404">If the user does not exist on the system</response>
        /// <remarks>
        /// Example:
        /// 
        ///     GET /api/v1/user/get-user-base-info
        /// 
        /// </remarks>
        [ProducesResponseType(typeof(BaseResponseWithData<UserBaseInfoResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponseWithData<UserBaseInfoResponseDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponseWithData<UserBaseInfoResponseDto>), StatusCodes.Status404NotFound)]
        [HttpGet("/api/v1/user/get-user-base-info")]
        [Authorize]
        public async Task<IActionResult> GetUserBaseInfoAsync()
        {
            var (statusCode, result) = await _userService.GetUserBaseInfoServiceAsync();
            return StatusCode(statusCode: statusCode, value: result);
        }



        /// <summary>
        /// This method handles the process of adding roles to a user.
        /// </summary>
        /// <param name="addUserRolesRequestDto">The DTO containing the user's ID and the list of roles to add.</param>
        /// <returns>Returns a success message if roles are successfully added to the user, otherwise returns an error message.</returns>
        /// <response code="200">Returns the success message if roles are successfully added to the user</response>
        /// <response code="400">If data is not valid</response>
        /// <response code="401">If user unauthorize</response>
        /// <response code="403">If the user does not have the necessary permissions to remove roles</response>
        /// <response code="404">If the user does not exist on the system</response>
        /// <response code="422">If an error occurred while adding the roles</response>
        /// <remarks>
        /// Example:
        /// 
        ///     POST /api/v1/user/add-user-roles
        ///     {
        ///         "UserId": "123",
        ///         "RoleNames": ["Role1", "Role2"]
        ///     }
        /// 
        /// </remarks>
        [ProducesResponseType(typeof(BaseResponseWithData<AddUserRolesResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponseWithData<AddUserRolesResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponseWithData<AddUserRolesResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponseWithData<AddUserRolesResponseDto>), StatusCodes.Status422UnprocessableEntity)]
        [HttpPost("/api/v1/user/add-user-roles")]
        // [Authorize(Roles = RolesConstant.Admin)]
        public async Task<IActionResult> AddUserRolesAsync(AddUserRolesRequestDto addUserRolesRequestDto)
        {
            if (ModelState.IsValid)
            {
                var (statusCode, result) = await _userService.AddUserRolesServiceAsync(
                    addUserRolesRequestDto: addUserRolesRequestDto);
                return StatusCode(statusCode: statusCode, value: result);
            }
            return BadRequest(ErrorsResult());
        }

        /// <summary>
        /// This method handles the process of removing roles from a user.
        /// </summary>
        /// <param name="removeUserRolesRequestDto">The DTO containing the user's ID and the list of roles to remove.</param>
        /// <response code="204">Returns the success message if roles are successfully removed from the user</response>
        /// <response code="400">If data is not valid</response>
        /// <response code="401">If user unauthorize</response>
        /// <response code="403">If the user does not have the necessary permissions to remove roles</response>
        /// <response code="404">If the user does not exist on the system</response>
        /// <response code="500">If an error occurred while removing the roles</response>
        /// <remarks>
        /// Example:
        /// 
        ///     POST /api/v1/user/remove-user-roles
        ///     {
        ///         "UserId": "123",
        ///         "RoleNames": ["Role1", "Role2"]
        ///     }
        /// 
        /// </remarks>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponseWithData<RemoveUserRolesResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponseWithData<RemoveUserRolesResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponseWithData<RemoveUserRolesResponseDto>), StatusCodes.Status500InternalServerError)]
        [HttpDelete("/api/v1/user/remove-user-roles")]
        // [Authorize(Roles = RolesConstant.Admin)]
        public async Task<IActionResult> RemoveUserRolesServiceAsync(
            RemoveUserRolesRequestDto removeUserRolesRequestDto)
        {
            if (ModelState.IsValid)
            {
                var (statusCode, result) = await _userService.RemoveUserRolesServiceAsync(
                removeUserRolesRequestDto: removeUserRolesRequestDto);
                return StatusCode(statusCode: statusCode, value: result);
            }
            return BadRequest(ErrorsResult());
        }



        private BaseResponseWithData<AddUserRolesResponseDto> ErrorsResult()
        {
            return new BaseResponseWithData<AddUserRolesResponseDto>()
            {
                IsSuccess = false,
                Message = "Invalid data",
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            };
        }
    }
}