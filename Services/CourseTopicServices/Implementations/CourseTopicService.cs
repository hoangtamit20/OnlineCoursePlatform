using Mapster;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.DTOs.CourseTopicDtos.Request;
using OnlineCoursePlatform.DTOs.CourseTopicDtos.Response;
using OnlineCoursePlatform.Helpers;
using OnlineCoursePlatform.Models.CourseTopicModels;
using OnlineCoursePlatform.Models.PagingAndFilter;
using OnlineCoursePlatform.Models.PagingAndFilter.Filter.CourseTopics;
using OnlineCoursePlatform.Repositories.CourseTopicRepositories.Interfaces;
using OnlineCoursePlatform.Repositories.CourseTypeRepositories;
using OnlineCoursePlatform.Services.CourseTopicServices.Interfaces;

namespace OnlineCoursePlatform.Services.CourseTopicServices.Implementations
{
    public class CourseTopicService : ICourseTopicService
    {
        private readonly ICourseTopicRepository _courseTopicRepository;
        private readonly ILogger<CourseTopicService> _logger;
        private readonly ICourseTypeRepository _courseTypeRepository;

        public CourseTopicService(
            ICourseTopicRepository courseTopicRepository,
            ICourseTypeRepository courseTypeRepository,
            ILogger<CourseTopicService> logger)
        {
            _courseTopicRepository = courseTopicRepository;
            _courseTypeRepository = courseTypeRepository;
            _logger = logger;
        }

        public async Task<PagedList<CourseTopicInfoModel>> GetAllsCoursesTopicsServiceAsync(
            CourseTopicFilterParams courseTopicFilterParams)
        => await _courseTopicRepository.GetAllsAsync(pagingAndFilterParams: courseTopicFilterParams);

        public async Task<PagedList<CourseTopicInfoModel>> GetAllsSecondFilterCourseTopicsServiceAsync(
            CourseTopicSecondFilterParams courseTopicSecondFilterParams)
        => await _courseTopicRepository.GetAllsSecondFilterAsync(
            courseTopicSecondFilterParams: courseTopicSecondFilterParams);



        public async Task<(int statusCode, BaseResponseWithData<CreateCourseTopicResponseDto> result)> CreateCourseTopicServiceAsync(
            CreateCourseTopicRequestDto createCourseTopicRequestDto)
        {
            // If data is valid

            // If course type referenced is exists
            var courseTypeExists = await _courseTypeRepository.FindCourseTypeByIdAsync(idCourseType: createCourseTopicRequestDto.CourseTypeId);
            if (courseTypeExists is null)
            {
                _logger.LogWarning($"Course type referenced not found");
                return BaseReturnHelper<CreateCourseTopicResponseDto>.GenerateErrorResponse(
                    errorMessage: $"Cannot found course type is referencing",
                    statusCode: StatusCodes.Status406NotAcceptable,
                    message: "Create course topic failed",
                    data: null
                );
            }
            // handle create course topic
            try
            {

                CourseTopic courseTopicCreated = await _courseTopicRepository.AddCourseTopicAsync(
                    courseTopic: createCourseTopicRequestDto.Adapt<CourseTopic>());
                return BaseReturnHelper<CreateCourseTopicResponseDto>.GenerateSuccessResponse(
                    data: courseTopicCreated.Adapt<CreateCourseTopicResponseDto>(),
                    message: "Create course topic success"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"The name of course topic was exists. Trace: {ex.Message}");
                return BaseReturnHelper<CreateCourseTopicResponseDto>.GenerateErrorResponse(
                    errorMessage: $"Unable to create. The course type name was exists",
                    statusCode: StatusCodes.Status409Conflict,
                    message: "Create course topic failed",
                    data: null
                );
            }
        }

        public async Task<(int statusCode, BaseResponseWithData<string> result)> DeleteCourseTopicAsync(int idCourseTopic)
        {
            // If course topic with id is not exists
            CourseTopic? courseTopicExists = await _courseTopicRepository.FindCourseTopicByIdAsync(id: idCourseTopic);
            if (courseTopicExists is null)
            {
                _logger.LogInformation($"Cannot found course topic with id : {idCourseTopic} while remove.");
                return BaseReturnHelper<string>.GenerateErrorResponse(
                    errorMessage: $"Cannot found course topic with id : {idCourseTopic}",
                    statusCode: StatusCodes.Status404NotFound,
                    message: "Delete course topic failed",
                    data: null
                );
            }
            // Handle delete course topic.
            try
            {
                await _courseTopicRepository.DeleteCourseTopicByIdAsync(courseTopic: courseTopicExists.Adapt<CourseTopic>());
                return BaseReturnHelper<string>.GenerateSuccessResponse(
                    data: $"The course topic with id : {idCourseTopic} was removed",
                    message: $"Delete course topic success");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Trace: {ex.Message}");
                return BaseReturnHelper<string>.GenerateErrorResponse(
                    errorMessage: "Unable to delete. The course topic is being referenced by other resources.",
                    statusCode: StatusCodes.Status409Conflict,
                    message: $"Delete course topic with id : {idCourseTopic} failed",
                    data: null
                );
            }
        }

        public async Task<(int statusCode, BaseResponseWithData<CourseTopicInfoModel> result)> GetCourseTopicServiceAsync(int id)
        {
            CourseTopic? courseTopicExists = await _courseTopicRepository.FindCourseTopicByIdAsync(id);
            if (courseTopicExists is null)
            {
                return BaseReturnHelper<CourseTopicInfoModel>.GenerateErrorResponse(
                    errorMessage: $"Course topic with id : {id} not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    message: "Get course topic failed",
                    data: null
                );
            }
            return BaseReturnHelper<CourseTopicInfoModel>.GenerateSuccessResponse(
                data: courseTopicExists.Adapt<CourseTopicInfoModel>(),
                message: "Get course topic success"
            );
        }

        public async Task<(int statusCode, BaseResponseWithData<UpdateCourseTopicResponseDto> result)> UpdateCourseTopicServiceAsync(
            UpdateCourseTopicRequestDto updateCourseTopicRequestDto)
        {
            var courseTopicExists = await _courseTopicRepository.FindCourseTopicByIdAsync(updateCourseTopicRequestDto.Id);
            var courseTypeExists = await _courseTypeRepository.FindCourseTypeByIdAsync(updateCourseTopicRequestDto.CourseTypeId);
            // If course topic is not exists or course type referenced not exists
            if (courseTopicExists is null || courseTypeExists is null)
            {
                _logger.LogWarning($"Course topic with id : {updateCourseTopicRequestDto.Id} not found or course type with id : {updateCourseTopicRequestDto.CourseTypeId} not found.");
                return BaseReturnHelper<UpdateCourseTopicResponseDto>.GenerateErrorResponse(
                    errorMessage: $"Course topic with id : {updateCourseTopicRequestDto.Id} not found or course type with id : {updateCourseTopicRequestDto.CourseTypeId} not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    message: "Update course topic failed",
                    data: null
                );
            }
            try
            {
                // If update course topic successed
                _logger.LogInformation($"Update course topic with id : {updateCourseTopicRequestDto.Id} success.");
                CourseTopic courseTopicUpdated = await _courseTopicRepository.UpdateCourseTopicAsync(
                    updateCourseTopicRequestDto.Adapt<CourseTopic>());
                return BaseReturnHelper<UpdateCourseTopicResponseDto>.GenerateSuccessResponse(
                    data: courseTopicUpdated.Adapt<UpdateCourseTopicResponseDto>(),
                    message: "Update course topic success"
                );
            }
            catch (Exception ex)
            {
                // If update course topic failed
                _logger.LogError($"Trace : {ex.Message}");
                return BaseReturnHelper<UpdateCourseTopicResponseDto>.GenerateErrorResponse(
                    errorMessage: $"CourseTopic's name has been used",
                    statusCode: StatusCodes.Status409Conflict,
                    message: "Update course topic failed",
                    data: null
                );
            }
        }
    }
}