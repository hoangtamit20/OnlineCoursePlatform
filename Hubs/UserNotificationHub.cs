using Mapster;
using Microsoft.AspNetCore.SignalR;
using OnlineCoursePlatform.Constants;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.Models.HubModels;

namespace OnlineCoursePlatform.Hubs
{
    public class UserNotificationHub : Hub
    {
        private readonly OnlineCoursePlatformDbContext _dbContext;

        public UserNotificationHub(
            OnlineCoursePlatformDbContext context)
        {
            _dbContext = context;
        }

        public async Task SendNotification(SendNotificationModel sendNotificationModel)
        {
            var notification = sendNotificationModel.Adapt<UserNotification>();
            _dbContext.UserNotifications.Add(notification);
            await _dbContext.SaveChangesAsync();

            await Clients.User(sendNotificationModel.UserId)
                .SendAsync(
                    method: HubConstants.ReceiveNotification,
                    arg1: sendNotificationModel.Message);
        }
    }
}