using OnlineCoursePlatform.Models.PagingAndFilter.Paging;

namespace OnlineCoursePlatform.Models.PagingAndFilter.Filter.CourseTypes
{
    public class CourseTypeFilterParams : PagingParams
    {
        public string? Name { get; set; }
    }
}