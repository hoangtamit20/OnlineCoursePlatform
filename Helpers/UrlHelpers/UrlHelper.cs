using System.Web;

namespace OnlineCoursePlatform.Helpers.UrlHelpers
{
    public static class UrlHelper
    {
        public static string BuildUrl(string baseUrl, Dictionary<string, string> queryParams)
        {
            var builder = new UriBuilder(baseUrl);
            var query = HttpUtility.ParseQueryString(builder.Query);
            foreach (var param in queryParams)
            {
                query[param.Key] = param.Value;
            }
            builder.Query = query.ToString();
            return builder.ToString();
        }
    }
}