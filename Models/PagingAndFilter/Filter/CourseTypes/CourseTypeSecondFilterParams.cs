using OnlineCoursePlatform.Models.PagingAndFilter.Paging;

namespace OnlineCoursePlatform.Models.PagingAndFilter.Filter.CourseTypes
{
    public class CourseTypeSecondFilterParams : PagingParams
    {
        public string FirstFilter { get; set; } = null!;
        public string? SecondFilter { get; set; }
    }
}