using Microsoft.AspNetCore.SignalR;
using OnlineCoursePlatform.Constants;

namespace OnlineCoursePlatform.Hubs
{
    public class ProgressHub : Hub
    {
        public string GetConnectionId()
            => Context.ConnectionId;

        public async Task SendProgressUploadFile(string progressMessage, string connectionId)
        {
            await Clients.Client(connectionId: connectionId).SendAsync(method: HubConstants.ReceiveProgress, arg1: progressMessage);
        }

        public async Task SendProgressSubmitFile(string jobId, string progressMessage, string connectionId)
        {
            await Clients.Client(connectionId: connectionId).SendAsync(method: HubConstants.ReceiveProgress, arg1: jobId, arg2: progressMessage);
        }
    }
}