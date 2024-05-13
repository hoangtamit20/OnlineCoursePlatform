using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.Data.Entities.Chat;
using OnlineCoursePlatform.DTOs.ChatDtos;
using OnlineCoursePlatform.DTOs.FileUploadDtos.Request;
using OnlineCoursePlatform.Models.UploadFileModels;
using OnlineCoursePlatform.Services.AzureBlobStorageServices;

namespace OnlineCoursePlatform.Services.ChatServices
{
    public class ChatService : IChatService
    {
        private readonly OnlineCoursePlatformDbContext _dbContext;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly UserManager<AppUser> _userManager;
        private readonly IAzureBlobStorageService _azureBlob;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            OnlineCoursePlatformDbContext dbContext,
            IHttpContextAccessor httpAccessor,
            UserManager<AppUser> userManager,
            IAzureBlobStorageService azureBlob,
            ILogger<ChatService> logger)
        {
            _dbContext = dbContext;
            _httpAccessor = httpAccessor;
            _userManager = userManager;
            _azureBlob = azureBlob;
            _logger = logger;
        }

        public async Task<ChatInfoDto?> AddMessageChatAsync(AddChatRequestDto requestDto)
        {
            if (requestDto.GroupChatId == null)
            {
                return null;
            }

            var currentUser = await GetCurrentUserAsync();

            if (currentUser == null)
            {
                return null;
            }

            // get group chat
            var groupChat = await _dbContext.GroupChats.FindAsync(requestDto.GroupChatId);

            if (groupChat is null)
            {
                return null;
            }

            var messageChat = new MessageChat()
            {
                GroupChatId = groupChat.Id,
                MessageText = requestDto.MessageText,
                SenderId = currentUser.Id,
                IsIncludedFile = !requestDto.UploadChatFileModels.IsNullOrEmpty()
            };

            _dbContext.MessageChats.Add(messageChat);
            try
            {
                await _dbContext.SaveChangesAsync();
                var chatFiles = new List<AttachmentOfMessageChat>();
                if (!requestDto.UploadChatFileModels.IsNullOrEmpty())
                {
                    // save chat files
                    requestDto.UploadChatFileModels.ForEach(cf =>
                    {
                        chatFiles.Add(new AttachmentOfMessageChat()
                        {
                            BlobContainerName = cf.BlobContainerName,
                            FileName = cf.FileName,
                            FileType = cf.FileType,
                            FileUrl = cf.FileUrl,
                            MessageChatId = messageChat.Id
                        });
                    });
                    _dbContext.AttachmentOfMessageChats.AddRange(entities: chatFiles);
                    await _dbContext.SaveChangesAsync();
                }
                var chatInfoDto = new ChatInfoDto()
                {
                    ChatFileInfos = messageChat.IsIncludedFile ? chatFiles.Select(cf => new ChatFileInfo()
                    {
                        Id = cf.Id,
                        FileName = cf.FileName,
                        FileType = cf.FileType,
                        FileUrl = cf.FileUrl
                    }).ToList() : new(),
                    MessageChatId = messageChat.Id,
                    MessageText = messageChat.MessageText,
                    Name = currentUser.Name,
                    Picture = currentUser.Picture,
                    SendDate = messageChat.SendDate,
                    UserId = currentUser.Id
                };
                return chatInfoDto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteMessageChatAsync(string messageChatId)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return false;
            }
            var messageChatExist = await _dbContext.MessageChats.FindAsync(messageChatId);
            if (messageChatExist == null)
            {
                return false;
            }
            if (messageChatExist.SenderId != currentUser.Id)
            {
                return false;
            }

            // delete mess
            _dbContext.MessageChats.Remove(messageChatExist);
            try
            {
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }


        public async Task<bool> DeleteMessageChatsAsync(List<string> messageChatIds)
        {
            var transaction = _dbContext.Database.BeginTransaction();
            {
                foreach (var item in messageChatIds)
                {
                    var result = await DeleteMessageChatAsync(item);
                    if (!result)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
                transaction.Commit();
                return true;
            }
        }

        public async Task<List<UploadChatFileModel>?> UploadChatFilesAsync(UploadChatFilesRequestDto
            requestDto)
        {
            if (requestDto.Files.IsNullOrEmpty())
            {
                return null;
            }

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return null;
            }

            List<UploadChatFileModel> result = await _azureBlob.UploadChatFiles(requestDto: requestDto, user: currentUser);
            return result;
        }

        public async Task<AppUser?> GetCurrentUserAsync()
        {
            ClaimsPrincipal? currentClaimsPrincipal = _httpAccessor.HttpContext?.User;
            if (currentClaimsPrincipal != null)
            {
                return await _userManager.GetUserAsync(principal: currentClaimsPrincipal);
            }
            return null;
        }
    }
}