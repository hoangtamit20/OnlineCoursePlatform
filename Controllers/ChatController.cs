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
            if (!string.IsNullOrEmpty(userIdModel.UserId) && !string.IsNullOrEmpty(userIdModel.GroupChatId))
            {
                return BadRequest(new BaseResponseWithData<List<ChatInfoDto>>()
                {
                    Errors = new List<string>() { $"Invalid data." },
                    IsSuccess = false,
                    Message = "Get conversation chats failed"
                });
            }
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
                GroupChat? conversation = null;
                if (!string.IsNullOrEmpty(userIdModel.UserId))
                {
                    conversation = await _dbContext.GroupChats
                    .Include(gc => gc.UserOfGroupChats)
                    .Where(g =>
                        g.Type == GroupChatType.Conversation &&
                        g.UserOfGroupChats.Any(u => u.UserId == currentUserId) &&
                        g.UserOfGroupChats.Any(u => u.UserId == userId))
                    .FirstOrDefaultAsync();
                }
                else
                {
                    conversation = await _dbContext.GroupChats.FindAsync(userIdModel.GroupChatId);
                }

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


        [HttpPost("/api/v1/chats/getorcreategroupchat")]
        [Authorize]
        public async Task<IActionResult> GetOrCreateGroupChat([FromBody] GroupChatModel requestDto)
        {
            if (requestDto.UserOfGroupChats.IsNullOrEmpty())
            {
                return BadRequest();
            }
            var currentUser = await _userManager.GetUserAsync(this.User);
            if (currentUser is null)
            {
                return Unauthorized();
            }
            if (string.IsNullOrEmpty(requestDto.GroupChatId))
            {
                var users = new List<AppUser>();
                requestDto.UserOfGroupChats.ForEach(s =>
                {
                    var user = _userManager.FindByIdAsync(s).Result;
                    if (user != null)
                    {
                        users.Add(user);
                    }
                });
                if (users.Where(u => u.Id == currentUser.Id).FirstOrDefault() != null)
                {
                    users.Remove(currentUser);
                }
                if (users.Count < 1)
                {
                    return BadRequest(new BaseResponseWithData<GroupChatInfoDto>()
                    {
                        Errors = new List<string>(){ $"The system does not support chatting with yourself" },
                        IsSuccess = false,
                        Data = null,
                        Message = "Get group chat failed"
                    });
                }
                // create group and add user to group
                GroupChat? group = null;
                if (users.Count == 1)
                {
                    group = await _dbContext.GroupChats
                    .Include(gc => gc.UserOfGroupChats)
                    .Where(g =>
                        g.Type == GroupChatType.Conversation &&
                        g.UserOfGroupChats.Any(u => u.UserId == currentUser.Id) &&
                        g.UserOfGroupChats.Any(u => users.Select(u => u.Id).Contains(u.UserId)))
                    .FirstOrDefaultAsync();
                    if (group == null)
                    {
                        group = new GroupChat()
                        {
                            Name = $"{currentUser.Name} and {users.First().Name}",
                            Type = GroupChatType.Conversation,
                        };
                    }
                    else
                    {
                        return Ok(new BaseResponseWithData<GroupChatInfoDto>()
                        {
                            Data = new GroupChatInfoDto()
                            {
                                GroupChatId = group.Id,
                                GroupChatName = group.Name
                            },
                            IsSuccess = true,
                            Message = "Get group chat successfully"
                        });
                    }
                }
                else
                {
                    group = new GroupChat()
                    {
                        AdminId = currentUser.Id,
                        Name = $"{currentUser.Name} and {users.Count} other peoples",
                        Type = GroupChatType.Group
                    };
                }
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

                    userOfGroupChats.AddRange(users.Select(u => new UserOfGroupChat()
                    {
                        GroupChatId = group.Id,
                        UserId = u.Id
                    }).ToList());

                    _dbContext.UserOfGroupChats.AddRange(userOfGroupChats);
                    await _dbContext.SaveChangesAsync();
                    return Ok(new BaseResponseWithData<GroupChatInfoDto>()
                    {
                        Data = new GroupChatInfoDto()
                        {
                            AdminId = group.AdminId,
                            GroupChatId = group.Id,
                            GroupChatName = group.Name
                        },
                        IsSuccess = true,
                        Message = "Create group chat sucessfully"
                    });

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
                }
            }
            else
            {
                var groupChat = await _dbContext.GroupChats.FindAsync(requestDto.GroupChatId);
                if (groupChat == null)
                {
                    return NotFound(new BaseResponseWithData<GroupChatInfoDto>()
                    {
                        Data = null,
                        IsSuccess = false,
                        Errors = new List<string>(){ $"Group chat with id : '{requestDto.GroupChatId}' not found." },
                        Message = "Get group chat failed"
                    });
                }
                return Ok(new BaseResponseWithData<GroupChatInfoDto>()
                {
                    Data = new GroupChatInfoDto()
                    {
                        AdminId = groupChat.AdminId,
                        GroupChatId = groupChat.Id,
                        GroupChatName = groupChat.Name
                    },
                    IsSuccess = true,
                    Message = "Get group chat success fully"
                });
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

        // GET: api/Chat/{groupId}
        [HttpGet("/api/v1/chats/{groupId}")]
        [Authorize]
        public async Task<ActionResult<ChatResponseDto>> GetMessagesByGroupId(string groupId)
        {
            var currentUser = await _userManager.GetUserAsync(this.User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            try
            {
                // Lấy thông tin nhóm chat
                var groupChatInfo = await _dbContext.GroupChats
                    .Where(g => g.Id == groupId)
                    .Select(g => new GroupChatInfoDto
                    {
                        GroupChatId = g.Id,
                        GroupChatName = g.Name,
                        AdminId = g.AdminId
                    })
                    .FirstOrDefaultAsync();

                if (groupChatInfo == null)
                {
                    return NotFound("Không tìm thấy nhóm chat.");
                }

                // Lấy tin nhắn của nhóm chat
                var chatInfos = await _dbContext.MessageChats
                    .Include(mc => mc.Sender)
                    .Include(mc => mc.AttachmentOfMessageChats)
                    .Where(m => m.GroupChatId == groupId)
                    .OrderBy(m => m.SendDate)
                    .Select(m => new ChatInfoDto
                    {
                        UserId = m.SenderId,
                        Name = m.Sender.Name, // Giả sử có một liên kết từ tin nhắn đến người dùng
                        Picture = m.Sender.Picture, // Tương tự, giả sử có một trường ảnh đại diện trong mô hình người dùng
                        IsCurrent = m.SenderId == currentUser.Id, // Kiểm tra xem tin nhắn có phải của người dùng hiện tại không
                        MessageText = m.MessageText,
                        MessageChatId = m.Id,
                        SendDate = m.SendDate,
                        ChatFileInfos = m.AttachmentOfMessageChats.Select(cf => new ChatFileInfo
                        {
                            Id = cf.Id,
                            FileUrl = cf.FileUrl,
                            FileName = cf.FileName,
                            FileType = cf.FileType
                        }).ToList()
                    })
                    .ToListAsync();

                // Tạo đối tượng ChatResponseDto và gán thông tin nhóm chat và tin nhắn vào
                var chatResponse = new ChatResponseDto
                {
                    GroupChatInfoDto = groupChatInfo,
                    ChatInfoDtos = chatInfos
                };

                return Ok(chatResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError,
                    value: new BaseResponseWithData<ChatResponseDto>()
                    {
                        Errors = new List<string>() { $"Error while get data of message chat." },
                        IsSuccess = false,
                        Message = "Get message chat failed"
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

    public class GetOrCreateGroupChatRequestDto
    {
        public string? GroupChatId { get; set; }
        public List<string> UserIds { get; set; } = new();
    }
}