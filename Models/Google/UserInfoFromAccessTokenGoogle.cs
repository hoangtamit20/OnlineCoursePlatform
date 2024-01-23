namespace OnlineCoursePlatform.Models.Google
{
    public class UserInfoFromIdTokenGoogle
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
    }
}