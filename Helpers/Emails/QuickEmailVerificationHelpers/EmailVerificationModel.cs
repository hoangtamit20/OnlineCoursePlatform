namespace OnlineCoursePlatform.Helpers.Emails.QuickEmailVerificationHelpers
{
    [Serializable]
    public class EmailVerificationModel
    {
        public string? result { get; set; }
        public string? reason { get; set; }
        public string? disposable { get; set; }
        public string? accept_all { get; set; }
        public string? role { get; set; }
        public string? free { get; set; }
        public string? email { get; set; }
        public string? user { get; set; }
        public string? domain { get; set; }
        public string? mx_record { get; set; }
        public string? mx_domain { get; set; }
        public string? safe_to_send { get; set; }
        public string? did_you_mean { get; set; }
        public string? success { get; set; }
        public object? message { get; set; }
    }
}