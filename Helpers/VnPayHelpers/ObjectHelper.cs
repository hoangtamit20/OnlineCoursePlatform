using System.Net;

namespace OnlineCoursePlatform.Helpers.VnPayHelpers
{
    public static class ObjectHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToQueryString(this object obj)
        {
            var properties = obj.GetType().GetProperties()
                .Where(o => o.GetValue(obj, null) != null)
                .Select(o => $"{o.Name}={WebUtility.UrlEncode(o.GetValue(obj, null)?.ToString())}");
            return string.Join("&", properties.ToArray());
        }
    }
}