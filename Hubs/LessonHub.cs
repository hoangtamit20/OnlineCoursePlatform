using Microsoft.AspNetCore.SignalR;

namespace OnlineCoursePlatform.Hubs
{
    public class LessonHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("ReceviceMessage", $"{Context.ConnectionId} has connected");
        }

    }
}