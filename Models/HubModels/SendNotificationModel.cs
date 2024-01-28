namespace OnlineCoursePlatform.Models.HubModels
{
    public class SendNotificationModel
    {
        public string UserId { get; set; } = null!;
        public string Message { get; set; } = null!;
        public int? OrderId { get; set; }
    }
}