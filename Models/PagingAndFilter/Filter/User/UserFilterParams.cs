using OnlineCoursePlatform.Models.PagingAndFilter.Paging;

namespace OnlineCoursePlatform.Models.PagingAndFilter.Filter.User
{
    public class UserFilterParams : PagingParams
    {
        public string? Query { get; set; }
        public UserFilterProperties? UserFilterProperties { get; set; }
    }
}