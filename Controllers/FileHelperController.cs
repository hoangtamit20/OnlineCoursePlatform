using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.DTOs.FileUploadDtos.Request;
using OnlineCoursePlatform.Services.AzureBlobStorageServices;

namespace OnlineCoursePlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileHelperController : ControllerBase
    {
        private readonly IAzureBlobStorageService _blobStorageService;
        private readonly UserManager<AppUser> _userManager;

        public FileHelperController(IAzureBlobStorageService blobStorageService,
        UserManager<AppUser> userManager)
        {
            _blobStorageService = blobStorageService;
            _userManager = userManager;
        }

        [HttpPost("/api/v1/files/upload-chat-files")]
        [Authorize]
        public async Task<IActionResult> UploadFiles(UploadChatFilesRequestDto requestDto)
        {
            var currentUser = await _userManager.GetUserAsync(this.User);
            if (currentUser is null)
            {
                return Unauthorized();
            }
            if (requestDto.Files is not null)
            {
                var result = await _blobStorageService.UploadChatFiles(requestDto: requestDto, user: currentUser);
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpDelete("/api/v1/files/delete")]
        [Authorize]
        public async Task<IActionResult> DeleteChatFiles()
        {
            var currentUser = await _userManager.GetUserAsync(this.User);
            if (currentUser is null)
                return Unauthorized();
            
        }

    }
}