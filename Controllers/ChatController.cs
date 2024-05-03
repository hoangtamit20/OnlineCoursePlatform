using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities.Chat;

namespace OnlineCoursePlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly OnlineCoursePlatformDbContext _dbContext;
        private readonly ILogger<ChatController> _logger;

        public ChatController(OnlineCoursePlatformDbContext dbContext, ILogger<ChatController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetABC(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId is not null)
            {
                // // check if current user and userid have exist group chat
                // var conversationChatOfCurrentUser = await _dbContext.UserOfGroupChats.Include(uo => uo.GroupChat)
                //     .Where(uo => uo.UserId == currentUserId && uo.GroupChat.Type == GroupChatType.Conversation)
                //     .ToListAsync();

                // foreach(var item in conversationChatOfCurrentUser)
                // {
                //     var isExist = await _dbContext.UserOfGroupChats.Where(uo => uo.GroupChatId == item.GroupChatId && uo.UserId == userId).FirstOrDefaultAsync();
                //     if (isExist is not null)
                //         break;
                // }

                var conversation = await _dbContext.GroupChats
                    .Include(gc => gc.UserOfGroupChats)
                    .Where(g =>
                        g.Type == GroupChatType.Conversation &&
                        g.UserOfGroupChats.Any(u => u.UserId == currentUserId) &&
                        g.UserOfGroupChats.Any(u => u.UserId == userId))
                    .FirstOrDefaultAsync();
                var currentUser = await _dbContext.Users.FindAsync(currentUserId);
                var userChat = await _dbContext.Users.FindAsync(userId);
                if (conversation is not null && currentUser is not null  && userChat is not null)
                {
                    // get list chat
                    var listChat = await _dbContext.MessageChats
                        .Include(mc => mc.AttachmentOfMessageChats)
                        .Where(mc => mc.GroupChatId == conversation.Id)
                        .Select(mc => new 
                        {
                            UserId = mc.SenderId,
                            Name = mc.SenderId == currentUserId ? currentUser.Name : userChat.Name,
                            Picture = mc.SenderId == currentUserId ? currentUser.Picture : userChat.Picture,
                            IsCurrent = mc.SenderId == currentUserId,
                            MessageChatId = mc.Id,
                            MessageText = mc.MessageText,
                            File = mc.IsIncludedFile ? mc.AttachmentOfMessageChats.Select(at => new {
                                FileUrl = at.FileUrl,
                                FileName = at.FileName,
                                FileType = at.FileType
                            }).ToList() : null,
                            SendDate = mc.SendDate
                        })
                        .OrderBy(g => g.SendDate)
                        .ToListAsync();
                }
                // Create group chat
                else
                {
                    var groupChat = new GroupChat()
                    {
                        Name = $"{currentUser?.Name} and {userChat?.Name}",
                        Type = GroupChatType.Conversation
                    };

                    _dbContext.GroupChats.Add(groupChat);
                    try
                    {
                        await _dbContext.SaveChangesAsync();
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(ex.Message);
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }
                }
            }


            


        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/v1/user/get-all-user-chat")]
        [Authorize]
        public async Task<IActionResult> GetUserChats()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (currentUserId is null)
            {
                return Unauthorized("Ivalid token");
            }
            else
            {
                return Ok(new BaseResponseWithData<List<UserDto>>()
                {
                    Data = await GetUsersInConversation(currentUserId),
                    IsSuccess = true,
                });
            }
        }

        private async Task<List<UserDto>> GetUsersInConversation(string currentUserId)
        {
            var users = await _dbContext.Users
                .Where(user => user.Id != currentUserId)
                .ToListAsync();

            var listUser = new List<UserDto>();
            var groupChatOfCurrentUser = _dbContext.UserOfGroupChats
                .Include(uo => uo.GroupChat)
                .Where(uo =>
                    uo.GroupChat.Type == GroupChatType.Conversation
                    && uo.UserId == currentUserId)
                .ToList();

            foreach (var user in users)
            {
                var groupChatOfUser = _dbContext.UserOfGroupChats
                .Include(uo => uo.GroupChat)
                .Where(uo =>
                    uo.GroupChat.Type == GroupChatType.Conversation
                    && uo.UserId == user.Id)
                .ToList();

                int count = 0;
                foreach (var item in groupChatOfUser)
                {
                    foreach (var itemCurrent in groupChatOfCurrentUser)
                    {
                        if (item.GroupChatId == itemCurrent.GroupChatId)
                        {
                            var messageExist = _dbContext.MessageChats
                                    .Where(mc => mc.GroupChatId == item.GroupChatId)
                                    .OrderByDescending(mc => mc.SendDate)
                                    .FirstOrDefault();
                            listUser.Add(new UserDto()
                            {
                                UserId = user.Id,
                                Email = user.Email!,
                                Picture = user.Picture,
                                Name = user.Name,
                                LatestMessageText = messageExist == null ? null : (messageExist.SenderId == currentUserId
                                    ? $"You : {messageExist.MessageText}" : messageExist.MessageText),
                                TimeAgo = messageExist == null ? null : GetTimeAgo(messageExist.SendDate)
                            });
                        }
                        count++;
                        break;
                    }
                    if (count > 0)
                        break;
                }
                if (count == 0)
                {
                    listUser.Add(new UserDto()
                    {
                        UserId = user.Id,
                        Email = user.Email!,
                        Picture = user.Picture,
                        Name = user.Name,
                        LatestMessageText = null,
                        TimeAgo = null
                    });
                }
            }
            return listUser;
        }

        private string GetTimeAgo(DateTime sendDate)
        {
            TimeSpan timeSpan = DateTime.UtcNow - sendDate;
            double totalMinutes = timeSpan.TotalMinutes;
            double totalHours = timeSpan.TotalHours;
            double totalDays = timeSpan.TotalDays;
            double totalWeeks = totalDays / 7;
            double totalMonths = totalDays / 30;
            double totalYears = totalDays / 365;

            if (totalYears >= 1)
            {
                return $"{Math.Round(totalYears)} year(s) ago";
            }
            else if (totalMonths >= 1)
            {
                return $"{Math.Round(totalMonths)} month(s) ago";
            }
            else if (totalWeeks >= 1)
            {
                return $"{Math.Round(totalWeeks)} week(s) ago";
            }
            else if (totalDays >= 1)
            {
                return $"{Math.Round(totalDays)} day(s) ago";
            }
            else if (totalHours >= 1)
            {
                return $"{Math.Round(totalHours)} hour(s) ago";
            }
            else if (totalMinutes >= 1)
            {
                return $"{Math.Round(totalMinutes)} minute(s) ago";
            }
            else
            {
                return "Just now";
            }
        }

    }


    public class UserDto
    {
        public string UserId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Email { get; set; }
        public string? Picture { get; set; }
        public string? LatestMessageText { get; set; }
        public string? TimeAgo { get; set; }
    }
}