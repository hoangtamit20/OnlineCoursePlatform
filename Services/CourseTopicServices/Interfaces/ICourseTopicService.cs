using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.DTOs.CourseTopicDtos.Request;
using OnlineCoursePlatform.DTOs.CourseTopicDtos.Response;
using OnlineCoursePlatform.Models.CourseTopicModels;
using OnlineCoursePlatform.Models.PagingAndFilter;
using OnlineCoursePlatform.Models.PagingAndFilter.Filter.CourseTopics;

namespace OnlineCoursePlatform.Services.CourseTopicServices.Interfaces
{
    public interface ICourseTopicService
    {
        Task<PagedList<CourseTopicInfoModel>> GetAllsCoursesTopicsServiceAsync(
            CourseTopicFilterParams courseTopicFilterParams);
        Task<PagedList<CourseTopicInfoModel>> GetAllsSecondFilterCourseTopicsServiceAsync(
        CourseTopicSecondFilterParams courseTopicSecondFilterParams);
        Task<(int statusCode, BaseResponseWithData<CreateCourseTopicResponseDto> result)> CreateCourseTopicServiceAsync(
            CreateCourseTopicRequestDto createCourseTopicRequestDto);
        Task<(int statusCode, BaseResponseWithData<string> result)> DeleteCourseTopicAsync(int id);
        Task<(int statusCode, BaseResponseWithData<CourseTopicInfoModel> result)> GetCourseTopicServiceAsync(int id);
        Task<(int statusCode, BaseResponseWithData<UpdateCourseTopicResponseDto> result)> UpdateCourseTopicServiceAsync(
            UpdateCourseTopicRequestDto updateCourseTopicRequestDto);
    }
}