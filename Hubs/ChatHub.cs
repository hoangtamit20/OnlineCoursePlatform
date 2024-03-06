using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OnlineCoursePlatform.Constants;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;

namespace OnlineCoursePlatform.Hubs
{
    // [Authorize]
    public class ChatHub : Hub
    {
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
    }
}