using Mapster;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.DTOs.CourseTypeDtos.Request;
using OnlineCoursePlatform.DTOs.CourseTypeDtos.Response;
using OnlineCoursePlatform.Helpers;
using OnlineCoursePlatform.Models.CourseTypeModels;
using OnlineCoursePlatform.Models.PagingAndFilter;
using OnlineCoursePlatform.Models.PagingAndFilter.Filter.CourseTypes;
using OnlineCoursePlatform.Repositories.CourseTypeRepositories;
using OnlineCoursePlatform.Services.CourseTypeServices.Interfaces;

namespace OnlineCoursePlatform.Services.CourseTypeServices.Implementations
{
    public class CourseTypeService : ICourseTypeService
    {
        private readonly ICourseTypeRepository _courseTypeRepository;
        private readonly ILogger<CourseTypeService> _logger;

        public CourseTypeService(
            ICourseTypeRepository courseTypeRepository,
            ILogger<CourseTypeService> logger)
        {
            _courseTypeRepository = courseTypeRepository;
            _logger = logger;
        }

        public async Task<(int statusCode, BaseResponseWithData<CreateCourseTypeResponseDto> result)> CreateCourseTypeServiceAsync(
            CreateCourseTypeRequestDto createCourseTypeRequestDto)
        {
            // If data is valid

            // If create CourseTypes success
            try
            {
                var createCourseType = await _courseTypeRepository.AddCourseTypeAsync(
                    courseType: createCourseTypeRequestDto.Adapt<CourseType>());
                return BaseReturnHelper<CreateCourseTypeResponseDto>.GenerateSuccessResponse(
                    data: createCourseType,
                    message: "Create course type success"
                );
            }catch(Exception ex)
            {
                _logger.LogError($"Trace : {ex.Message}");
                return BaseReturnHelper<CreateCourseTypeResponseDto>.GenerateErrorResponse(
                    errorMessage: $"Unable to create. The course type name was exists",
                    statusCode: StatusCodes.Status409Conflict,
                    message: "Create course type failed",
                    data: null
                );
            }
        }

        public async Task<(int statusCode, BaseResponseWithData<string> result)> DeleteCourseTypeByIdServiceAsync(int idCourseType)
        {
            // If CourseType not exists
            var courseTypeExists = await _courseTypeRepository.FindCourseTypeByIdAsync(idCourseType: idCourseType);
            if (courseTypeExists is null)
            {
                return BaseReturnHelper<string>.GenerateErrorResponse(
                    errorMessage: $"Course type with id : {idCourseType} not found",
                    statusCode: StatusCodes.Status404NotFound,
                    message: $"Delete course type with id : {idCourseType} failed",
                    data: null
                );
            }
            // handle delete course type
            try{
                await _courseTypeRepository.DeleteCourseTypeByIdAsync(courseType: courseTypeExists.Adapt<CourseType>());
                return BaseReturnHelper<string>.GenerateSuccessResponse(
                    data: $"The course type with id : {idCourseType} was removed",
                    message: $"Delete course type success");
            }catch(Exception ex)
            {
                _logger.LogError($"Trace: {ex.Message}");
                return BaseReturnHelper<string>.GenerateErrorResponse(
                    errorMessage: "Unable to delete. The course type is being referenced by other resources.",
                    statusCode: StatusCodes.Status409Conflict,
                    message: $"Delete course type with id : {idCourseType} failed",
                    data: null
                );
            }
        }

        public async Task<(int statusCode, BaseResponseWithData<CourseTypeInfoModel> result)> FindCourseTypeByIdServiceAsync(int idCourseType)
        {
            var courseTypeInfoModel = await _courseTypeRepository.FindCourseTypeByIdAsync(idCourseType: idCourseType);
            // If courseType not found
            if (courseTypeInfoModel is null)
            {
                return BaseReturnHelper<CourseTypeInfoModel>.GenerateErrorResponse(
                    errorMessage: $"Course type with id: {idCourseType} not found",
                    statusCode: StatusCodes.Status404NotFound,
                    message: "Get course type failed",
                    data: null
                );
            }
            return BaseReturnHelper<CourseTypeInfoModel>.GenerateSuccessResponse(
                data: courseTypeInfoModel,
                message: $"Get course type with id: {idCourseType} success.");
        }

        public async Task<PagedList<CourseTypeInfoModel>> GetAllsCoursesTypeServiceAsync(
            CourseTypeFilterParams courseTypeFilterParams)
        => await _courseTypeRepository.GetAllsAsync(courseTypeFilterParams: courseTypeFilterParams);

        public async Task<PagedList<CourseTypeInfoModel>> GetAllsSecondFilterCourseTypesServiceAsync(
            CourseTypeSecondFilterParams courseTypeSecondFilterParams)
        => await _courseTypeRepository.GetAllsSecondFilterAsync(
            courseTypeSecondFilterParams: courseTypeSecondFilterParams);

        public async Task<(int statusCode, BaseResponseWithData<UpdateCourseTypeResponseDto> result)> UpdateCourseTypeServiceAsync(
            UpdateCourseTypeRequestDto updateCourseTypeRequestDto)
        {
            var courseTypeExists = await _courseTypeRepository.FindCourseTypeByIdAsync(updateCourseTypeRequestDto.Id);
            // If course type is exists
            if (courseTypeExists is null)
            {
                return BaseReturnHelper<UpdateCourseTypeResponseDto>.GenerateErrorResponse(
                    errorMessage: "The CourseTypes not found",
                    statusCode: StatusCodes.Status404NotFound,
                    message: "Update course type failed",
                    data: null
                );
            }
            try{
                var courseType = await _courseTypeRepository
                .UpdateCourseTypeAsync(updateCourseTypeRequestDto.Adapt<CourseType>());
                return BaseReturnHelper<UpdateCourseTypeResponseDto>.GenerateSuccessResponse(
                    data: courseType.Adapt<UpdateCourseTypeResponseDto>(),
                    message: "Update course type success"
                );
            }catch(Exception ex)
            {
                return BaseReturnHelper<UpdateCourseTypeResponseDto>.GenerateErrorResponse(
                    errorMessage: $"Unable to update. {ex.Message}",
                    statusCode: StatusCodes.Status409Conflict,
                    message: "Update course type failed",
                    data: null
                );
            }
        }
    }
}