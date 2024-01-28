using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.DTOs.CourseTypeDtos.Request;
using OnlineCoursePlatform.DTOs.CourseTypeDtos.Response;
using OnlineCoursePlatform.Models.CourseTypeModels;
using OnlineCoursePlatform.Models.PagingAndFilter;
using OnlineCoursePlatform.Models.PagingAndFilter.Filter.CourseTypes;

namespace OnlineCoursePlatform.Services.CourseTypeServices.Interfaces
{
    public interface ICourseTypeService
    {
        Task<PagedList<CourseTypeInfoModel>> GetAllsCoursesTypeServiceAsync(
            CourseTypeFilterParams courseTypeFilterParams);
        Task<PagedList<CourseTypeInfoModel>> GetAllsSecondFilterCourseTypesServiceAsync(
            CourseTypeSecondFilterParams courseTypeSecondFilterParams);
        Task<(int statusCode, BaseResponseWithData<CourseTypeInfoModel> result)> FindCourseTypeByIdServiceAsync(int idCourseType);
        Task<(int statusCode, BaseResponseWithData<string> result)> DeleteCourseTypeByIdServiceAsync(int idCourseType);
        Task<(int statusCode, BaseResponseWithData<CreateCourseTypeResponseDto> result)> CreateCourseTypeServiceAsync(
            CreateCourseTypeRequestDto createCourseTypeRequestDto);
        Task<(int statusCode, BaseResponseWithData<UpdateCourseTypeResponseDto> result)> UpdateCourseTypeServiceAsync(
            UpdateCourseTypeRequestDto updateCourseTypeRequestDto);
    }
}