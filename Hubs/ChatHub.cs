using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OnlineCoursePlatform.Constants;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.Data.Entities.Chat;

namespace OnlineCoursePlatform.Hubs
{
    // [Authorize]
    public class ChatHub : Hub
    {
        private readonly OnlineCoursePlatformDbContext _dbContext;

        public ChatHub(OnlineCoursePlatformDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SendMessageToUser(
            string message)

        {
            await Clients.All
                .SendAsync(HubConstants.MessageHelper, message);
        }

        public async Task SendComments(string message)
        {
            await Clients.All
                .SendAsync(HubConstants.ReceiveNotification, message);
        }

        // public async Task SendMessage(string message)
        // {
        //     var user = Context.User;

        //     if (user.IsInRole("Admin"))
        //     {
        //         // Send a message to all users
        //         await Clients.All.SendAsync("ReceiveMessage", message);
        //     }
        //     else
        //     {
        //         // Send a message only to the admin
        //         await Clients.User(adminUserId).SendAsync("ReceiveMessage", message);
        //     }
        // }

        // public async Task SendMessageToAdmin(
        //     // string userIdAdmin,
        //     string message)
        // {
        //     // Send a message to the admin
        //     await Clients
        //         .All
        //         // .User(userId: userIdAdmin)
        //         .SendAsync(method: HubConstants.MessageHelper, arg1: message);
        // }



        // // Dictionary để lưu trữ thông tin về người dùng đang kết nối (UserId và ConnectionId)
        // private static Dictionary<string, string> _onlineUsers = new Dictionary<string, string>();

        // public override async Task OnConnectedAsync()
        // {
        //     // Lấy UserId của người dùng từ context
        //     var userId = Context.UserIdentifier;

        //     // Lấy ConnectionId của người dùng từ context
        //     var connectionId = Context.ConnectionId;

        //     // Thêm UserId và ConnectionId vào danh sách người dùng đang kết nối
        //     _onlineUsers[userId] = connectionId;

        //     // Gửi thông báo hoặc thực hiện các thao tác khác khi người dùng kết nối thành công

        //     await base.OnConnectedAsync();
        // }

        // public override async Task OnDisconnectedAsync(Exception exception)
        // {
        //     // Lấy UserId của người dùng từ context
        //     var userId = Context.UserIdentifier;

        //     // Xóa người dùng khỏi danh sách người dùng đang kết nối khi người dùng ngắt kết nối
        //     _onlineUsers.Remove(userId);

        //     // Gửi thông báo hoặc thực hiện các thao tác khác khi người dùng ngắt kết nối

        //     await base.OnDisconnectedAsync(exception);
        // }




        public override async Task OnConnectedAsync()
        {
            var currentUser = await GetUserFromJwtAsync();
            if (currentUser != null)
            {
                var connectionId = Context.ConnectionId;

                // Lấy số lượng tin nhắn mới và thông báo mới chưa đọc cho người dùng
                var unreadMessage = await GetMessageUnRead(currentUser.Id);
                var unreadNofti = await GetNoftiUnRead(currentUser.Id);

                var newMessage = await GetNewMessageNoftiAsync(currentUser.Id);
                var newNofti = await GetNewNoftiAsync(currentUser.Id);



                // Gửi thông báo toast cho người dùng với nội dung mới nếu có
                    var toastMessage = $"You have {newMessage.Count} new messages and {newNofti.Count} new notifications";
                    await Clients.Client(connectionId).SendAsync("ToastNewNotifications", toastMessage);

                // Gửi số lượng tin nhắn mới và thông báo mới chưa đọc cho người dùng
                await Clients.Client(connectionId).SendAsync("SendQuantityOfUnReadMessages", unreadMessage.Count);
                await Clients.Client(connectionId).SendAsync("SendQuantityOfUnReadNoftifications", unreadNofti.Count);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnConnectedAsync();
        }



        private async Task<AppUser?> GetUserFromJwtAsync()
        {
            var jwtToken = Context.GetHttpContext()?.Request.Query["access_token"].ToString();
            if (!string.IsNullOrEmpty(jwtToken))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(jwtToken);
                var userId = jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
                var userExist = await _dbContext.Users.FindAsync(userId);
                return userExist;
            }
            return null;
        }


        private async Task<IEnumerable<MessageChat>> GetUnsentMessages(string userId)
        {
            // Query the database to get the unsent messages for the given user ID
            var unsentMessages = await _dbContext.WaitingMessageChats
                .Where(w => w.UserId == userId)
                .Select(w => w.MessageChat)
                .ToListAsync();
            return unsentMessages;
        }


        private async Task<List<UserNotification>> GetNewMessageNoftiAsync(string userId)
        {
            var result = await _dbContext.UserNotifications
                .Where(un => un.UserId == userId
                    && !string.IsNullOrEmpty(un.MessageChatId)
                    && !un.IsReceived)
                .ToListAsync();
            return result;
        }

        private async Task<List<UserNotification>> GetNewNoftiAsync(string userId)
        {
            var result = await _dbContext.UserNotifications
                .Where(un => un.UserId == userId
                    && un.OrderId != null
                    && !un.IsReceived)
                .ToListAsync();
            return result;
        }


        private async Task<List<UserNotification>> GetMessageUnRead(string userId)
        {
            var result = await _dbContext.UserNotifications
                .Where(un => un.UserId == userId
                    && !string.IsNullOrEmpty(un.MessageChatId)
                    && !un.IsRead)
                .ToListAsync();
            return result;
        }

        private async Task<List<UserNotification>> GetNoftiUnRead(string userId)
        {
            var result = await _dbContext.UserNotifications
                .Where(un => un.UserId == userId
                    && !string.IsNullOrEmpty(un.MessageChatId)
                    && !un.IsRead)
                .ToListAsync();
            return result;
        }
    }
}