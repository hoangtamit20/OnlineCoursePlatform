using OnlineCoursePlatform.Models.PagingAndFilter.Paging;

namespace OnlineCoursePlatform.Models.PagingAndFilter.Filter.CourseTopics
{
    public class CourseTopicSecondFilterParams : PagingParams
    {
        public string FirstFilter { get; set; } = null!;
        public string? SecondFilter { get; set; }
    }
}