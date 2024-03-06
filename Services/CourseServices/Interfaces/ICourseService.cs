using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.DTOs.CourseDtos.Request;
using OnlineCoursePlatform.DTOs.CourseDtos.Response;
using OnlineCoursePlatform.Models.CourseModels;
using OnlineCoursePlatform.Models.PagingAndFilter;
using OnlineCoursePlatform.Models.PagingAndFilter.Filter.Course;

namespace OnlineCoursePlatform.Services.CourseServices.Interfaces
{
    public interface ICourseService
    {
        Task<PagedList<CourseInfoModel>> GetAllsCourseServiceAsync(
            CourseFilterParams courseFilterParams);
        Task<PagedList<CourseInfoModel>> GetAllsCourseSecondFilterServiceAsync(
            CourseSecondFilterParams courseSecondFilterParams);
        
        Task<(int statusCode, BaseResponseWithData<CreateCourseResponseDto> result)> CreateCourseServiceAsync(
            CreateCourseRequestDto createCourseRequestDto);
        
        Task<(int statusCode, BaseResponseWithData<CourseDetailResponseDto> result)> GetCourseDetailServiceAsync(int courseId);

        
    }
}