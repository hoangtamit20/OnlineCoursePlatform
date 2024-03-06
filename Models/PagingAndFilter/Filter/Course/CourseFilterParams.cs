using OnlineCoursePlatform.Models.PagingAndFilter.Paging;

namespace OnlineCoursePlatform.Models.PagingAndFilter.Filter.Course
{
    public class CourseFilterParams : PagingParams
    {
        public string? Query { get; set; }
        public CourseFilterProperties? CourseFilterProperties { get; set; }

    }
}