using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCoursePlatform.DTOs.FileUploadDtos.Request;
using OnlineCoursePlatform.Services.ChatServices;

namespace OnlineCoursePlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IChatService _chatService;

        public FileController(IChatService chatService)
        {
            _chatService = chatService;
        }


        [HttpPost("/api/v1/files/uploadchatfiles")]
        [Authorize]
        public async Task<IActionResult> UploadChatFiles(UploadChatFilesRequestDto requestDto)
        {
            var result = await _chatService.UploadChatFilesAsync(requestDto: requestDto);
            if (result is null)
            {
                return BadRequest();
            }
            return Ok(result);
        }


        // [HttpDelete("/api/v1/files/deletechatfiles")]
        // [Authorize]
        // public async Task<IActionResult> DeleteChatFiles()
        // {
        //     return Ok();
        // }
    }
}