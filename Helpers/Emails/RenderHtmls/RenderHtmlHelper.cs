namespace OnlineCoursePlatform.Helpers.Emails.RenderHtmls
{
    public static class RenderHtmlHelper
    {
        public static string GetHtmlConfirmEmail(Uri urlConfirm) => $@"{urlConfirm}";
    }
}