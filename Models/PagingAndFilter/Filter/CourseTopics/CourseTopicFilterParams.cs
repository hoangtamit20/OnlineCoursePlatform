using OnlineCoursePlatform.Models.PagingAndFilter.Paging;

namespace OnlineCoursePlatform.Models.PagingAndFilter.Filter.CourseTopics
{
    public class CourseTopicFilterParams : PagingParams
    {
        public string? Name { get; set; }
    }
}