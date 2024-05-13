using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.Data.Entities.Chat;
using OnlineCoursePlatform.DTOs.ChatDtos;

namespace OnlineCoursePlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly OnlineCoursePlatformDbContext _dbContext;
        private readonly ILogger<ChatController> _logger;
        private readonly UserManager<AppUser> _userManager;

        public ChatController(UserManager<AppUser> userManager, OnlineCoursePlatformDbContext dbContext, ILogger<ChatController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
            _userManager = userManager;
        }


        [HttpPost("/api/v1/chats/getconversationchats")]
        [Authorize]
        [ProducesResponseType(typeof(ChatResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetConversationChats([FromBody] UserIdModel userIdModel)
        {
            var userId = userIdModel.UserId;
            if (userId is null)
            {
                return Unauthorized();
            }
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var groupChatInfoDto = new GroupChatInfoDto();
            var listChats = new List<ChatInfoDto>();
            if (currentUserId is not null)
            {
                var conversation = await _dbContext.GroupChats
                    .Include(gc => gc.UserOfGroupChats)
                    .Where(g =>
                        g.Type == GroupChatType.Conversation &&
                        g.UserOfGroupChats.Any(u => u.UserId == currentUserId) &&
                        g.UserOfGroupChats.Any(u => u.UserId == userId))
                    .FirstOrDefaultAsync();
                var currentUser = await _dbContext.Users.FindAsync(currentUserId);
                var userChat = await _dbContext.Users.FindAsync(userId);
                if (currentUser is null)
                {
                    return Unauthorized();
                }
                if (userChat is null)
                {
                    return Unauthorized();
                }
                // If exist conversation between two user
                if (conversation is not null && currentUser is not null && userChat is not null)
                {
                    // get list chat
                    groupChatInfoDto.GroupChatId = conversation.Id;
                    groupChatInfoDto.GroupChatName = conversation.Name;
                    groupChatInfoDto.AdminId = conversation.AdminId;
                    listChats = await _dbContext.MessageChats
                        .Include(mc => mc.AttachmentOfMessageChats)
                        .Where(mc => mc.GroupChatId == conversation.Id)
                        .Select(mc => new ChatInfoDto()
                        {
                            UserId = mc.SenderId,
                            Name = mc.SenderId == currentUserId ? currentUser.Name : userChat.Name,
                            Picture = mc.SenderId == currentUserId ? currentUser.Picture : userChat.Picture,
                            IsCurrent = mc.SenderId == currentUserId,
                            MessageChatId = mc.Id,
                            MessageText = mc.MessageText,
                            ChatFileInfos = mc.IsIncludedFile ? mc.AttachmentOfMessageChats.Select(at => new ChatFileInfo
                            {
                                FileUrl = at.FileUrl,
                                FileName = at.FileName,
                                FileType = at.FileType
                            }).ToList() : new List<ChatFileInfo>(),
                            SendDate = mc.SendDate
                        })
                        .OrderBy(g => g.SendDate)
                        .ToListAsync();
                }
                // Create group chat and add user to user of group chat
                else
                {
                    var groupChat = new GroupChat()
                    {
                        Name = $"{currentUser!.Name} and {userChat!.Name}",
                        Type = GroupChatType.Conversation
                    };

                    _dbContext.GroupChats.Add(groupChat);
                    try
                    {

                        await _dbContext.SaveChangesAsync();
                        groupChatInfoDto.GroupChatId = groupChat.Id;
                        groupChatInfoDto.GroupChatName = groupChat.Name;
                        // add user to userofgroupchat
                        _dbContext.UserOfGroupChats.AddRange(entities:
                        new List<UserOfGroupChat>()
                        {
                            new UserOfGroupChat()
                            {
                                GroupChatId = groupChat.Id,
                                UserId = currentUser!.Id,
                            },
                            new UserOfGroupChat()
                            {
                                GroupChatId = groupChat.Id,
                                UserId = userChat!.Id
                            }
                        });
                        await _dbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }
                }
            }

            return Ok(new ChatResponseDto()
            {
                GroupChatInfoDto = groupChatInfoDto,
                ChatInfoDtos = listChats
            });
        }


        [HttpPost("getgroupchats")]
        [Authorize]
        public async Task<IActionResult> GetGroupChats([FromBody] GroupChatModel groupChat)
        {
            if (groupChat.UserOfGroupChats.IsNullOrEmpty())
            {
                return BadRequest();
            }
            var currentUser = await _userManager.GetUserAsync(this.User);
            if (currentUser is null)
            {
                return Unauthorized();
            }
            if (string.IsNullOrEmpty(groupChat.GroupChatId))
            {
                var listUser = new List<AppUser>();
                groupChat.UserOfGroupChats.ForEach(s =>
                {
                    var user = _userManager.FindByIdAsync(s).Result;
                    if (user != null)
                    {
                        listUser.Add(user);
                    }
                });
                // create group and add user to group
                var group = new GroupChat()
                {
                    AdminId = currentUser.Id,
                    Name = $"{currentUser.Name} and {listUser.Count} other peoples",
                    Type = GroupChatType.Group
                };

                _dbContext.GroupChats.Add(group);
                try
                {
                    await _dbContext.SaveChangesAsync();
                    var userOfGroupChats = new List<UserOfGroupChat>()
                    {
                        new UserOfGroupChat()
                        {
                            GroupChatId = group.Id,
                            UserId = currentUser.Id,
                        }
                    };

                    userOfGroupChats.AddRange(listUser.Select(u => new UserOfGroupChat()
                    {
                        GroupChatId = group.Id,
                        UserId = u.Id
                    }).ToList());

                    _dbContext.UserOfGroupChats.AddRange(userOfGroupChats);
                    await _dbContext.SaveChangesAsync();
                    return Ok();
                    
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
                }
            }
            return BadRequest();
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