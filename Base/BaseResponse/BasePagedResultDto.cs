using OnlineCoursePlatform.Models.PagingAndFilter.Filter.Course;

namespace OnlineCoursePlatform.Base.BaseResponse
{
    public class BasePagedResultDto<T>
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
        public string? FirstFilter { get; set; }
        public CourseFilterProperties? CourseFilterProperties { get; set; }
        public List<T> Items { get; set; } = new();
    }
}