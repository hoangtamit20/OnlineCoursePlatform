using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Constants;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.DTOs.AzureDtos.Request;
using OnlineCoursePlatform.DTOs.AzureDtos.Response;
using OnlineCoursePlatform.DTOs.CourseDtos.Request;
using OnlineCoursePlatform.DTOs.CourseDtos.Response;
using OnlineCoursePlatform.Helpers;
using OnlineCoursePlatform.Helpers.Uploads;
using OnlineCoursePlatform.Hubs;
using OnlineCoursePlatform.Models.CourseModels;
using OnlineCoursePlatform.Models.PagingAndFilter;
using OnlineCoursePlatform.Models.PagingAndFilter.Filter.Course;
using OnlineCoursePlatform.Models.UploadFileModels;
using OnlineCoursePlatform.Repositories.CourseRepositories;
using OnlineCoursePlatform.Services.AzureBlobStorageServices;
using OnlineCoursePlatform.Services.AzureMediaServices;
using OnlineCoursePlatform.Services.CourseServices.Interfaces;

namespace OnlineCoursePlatform.Services.CourseServices.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IAzureMediaService _azureMediaService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        // private readonly OnlineCoursePlatformDbContext _dbContext;
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly ILogger<CourseService> _logger;
        private readonly IHubContext<ProgressHub> _hubContext;

        public CourseService(
            ICourseRepository courseRepository,
            IAzureMediaService azureMediaService,
            IAzureBlobStorageService azureBlobStorageService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<CourseService> logger,
            IHubContext<ProgressHub> hubContext)
        {
            _courseRepository = courseRepository;
            _azureMediaService = azureMediaService;
            _azureBlobStorageService = azureBlobStorageService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _hubContext = hubContext;
            // _dbContext = dbContext;
        }


        public async Task<PagedList<CourseInfoModel>> GetAllsCourseServiceAsync(
            CourseFilterParams courseFilterParams)
        => await _courseRepository.GetAllsAsync(pagingAndFilterParams: courseFilterParams);

        public async Task<PagedList<CourseInfoModel>> GetAllsCourseSecondFilterServiceAsync(
            CourseSecondFilterParams courseSecondFilterParams)
        => await _courseRepository.GetAllsSecondFilterAsync(courseSecondFilterParams: courseSecondFilterParams);


        public async Task<(int statusCode, BaseResponseWithData<CreateCourseResponseDto> result)> CreateCourseServiceAsync(
            CreateCourseRequestDto createCourseRequestDto)
        {
            // If user is not exists
            var userId = GetCurrentUserId();
            var connectionId = GetConnectionIdOfSignalRHub();
            if (userId is null || connectionId is null)
            {
                return BaseReturnHelper<CreateCourseResponseDto>.GenerateErrorResponse(
                    errorMessage: "Unauthorized. Cannot found any user.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized",
                    data: null
                );
            }
            var userExists = await _courseRepository.FindUserByIdAsync(userId: userId);
            if (userExists is null)
            {
                return BaseReturnHelper<CreateCourseResponseDto>.GenerateErrorResponse(
                    errorMessage: "Unauthorized. Cannot found any user.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized",
                    data: null
                );
            }
            // Begin transaction to create course, coursesubtitles, courseurlstreaming
            using var _transaction = await _courseRepository.CreateTransactionAsync();
            {
                // If create course failed
                if (!string.IsNullOrEmpty(connectionId) && !string.IsNullOrWhiteSpace(connectionId))
                {
                    await _hubContext.Clients
                        .Client(connectionId: connectionId)
                        .SendAsync(method: HubConstants.ReceiveProgress, arg1: "Create course ....");
                }
                var course = new Course()
                {
                    Name = createCourseRequestDto.Name,
                    ExpirationDay = createCourseRequestDto.ExpirationDay,
                    Price = createCourseRequestDto.Price,
                    CourseTopicId = createCourseRequestDto.CourseTopicId,
                    UserId = userExists.Id,
                    CourseDescription = createCourseRequestDto.CourseDescription
                };
                Course courseCreated;
                try
                {
                    courseCreated = await _courseRepository.CreateCourseAsync(course: course);
                    if (!string.IsNullOrEmpty(connectionId) && !string.IsNullOrWhiteSpace(connectionId))
                    {
                        await _hubContext.Clients.Client(connectionId: connectionId).SendAsync(
                            method: HubConstants.ReceiveProgress,
                            arg1: "Create course finished");
                    }

                }
                catch (Exception ex)
                {
                    await _transaction.RollbackAsync();
                    return BaseReturnHelper<CreateCourseResponseDto>.GenerateErrorResponse(
                        errorMessage: $"{ex.Message}",
                        statusCode: StatusCodes.Status400BadRequest,
                        message: "Create course failed",
                        data: null
                    );
                }
                // If file upload thumbnail is not null
                // var uploadThumbnailResult = await UploadPublicFileHelper.UploadThumbnailFileAsync(
                //     hubContext: _hubContext,
                //     connectionId: connectionId,
                //     user: userExists,
                //     azureBlobStorageService: _azureBlobStorageService,
                //     courseOrLessonObj: course,
                //     fileToUpload: createCourseRequestDto.ThumbnailFileUpload,
                //     transaction: _transaction,
                //     logger: _logger,
                //     courseRepository: _courseRepository);
                // if (uploadThumbnailResult is not null)
                // {
                //     return BaseReturnHelper<CreateCourseResponseDto>.GenerateErrorResponse(
                //         errorMessage: uploadThumbnailResult.Value.errorMessage,
                //         statusCode: uploadThumbnailResult.Value.statusCode,
                //         message: "Create course failed",
                //         data: null);
                // }
                if (createCourseRequestDto.ThumbnailFileUpload is not null)
                {
                    if (!string.IsNullOrEmpty(connectionId) && !string.IsNullOrWhiteSpace(connectionId))
                    {
                        await _hubContext.Clients.Client(connectionId: connectionId).SendAsync(
                            method: HubConstants.ReceiveProgress,
                            arg1: "Upload thumbnail ....");
                    }
                    UploadPublicFileModel? uploadThumbnailModel = null;
                    try
                    {
                        uploadThumbnailModel = (await _azureBlobStorageService.UploadPublicFilesToAzureBlobStorageAsync(
                            user: userExists,
                            courseId: course.Id.ToString(),
                            lessonId: null,
                            fileThumbnail: createCourseRequestDto.ThumbnailFileUpload,
                            fileSubtitles: null
                        )).FirstOrDefault();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"{ex.Message}");
                        await _transaction.RollbackAsync();
                        return BaseReturnHelper<CreateCourseResponseDto>.GenerateErrorResponse(
                            errorMessage: $"Upload thumbnail failed",
                            statusCode: StatusCodes.Status500InternalServerError,
                            message: "Create course failed",
                            data: null
                        );
                    }
                    if (uploadThumbnailModel is null)
                    {
                        await _transaction.RollbackAsync();
                        return BaseReturnHelper<CreateCourseResponseDto>.GenerateErrorResponse(
                            errorMessage: $"Upload thumbnail failed",
                            statusCode: StatusCodes.Status400BadRequest,
                            message: "Create course failed",
                            data: null
                        );
                    }
                    // Update thumbnail for course

                    course.Thumbnail = uploadThumbnailModel.FileUrl;
                    course.BlobContainerName = uploadThumbnailModel.ContainerName;
                    course.FileThumbnailName = uploadThumbnailModel.FileName;
                    try
                    {
                        await _courseRepository.UpdateCourseAsync(course: course);
                        if (!string.IsNullOrEmpty(connectionId) && !string.IsNullOrWhiteSpace(connectionId))
                        {
                            await _hubContext.Clients.Client(connectionId: connectionId).SendAsync(
                                method: HubConstants.ReceiveProgress,
                                arg1: "Upload thumbnail finished.");
                        }
                    }
                    catch (Exception ex)
                    {
                        await _transaction.RollbackAsync();
                        return BaseReturnHelper<CreateCourseResponseDto>.GenerateErrorResponse(
                            errorMessage: $"{ex.Message}",
                            statusCode: StatusCodes.Status400BadRequest,
                            message: "Create course failed",
                            data: null
                        );
                    }
                }
                // If file upload subtitle for course is not null
                List<CourseSubtitle>? listCourseSubtitles = null;
                // var uploadSubtitlesFileResult = await UploadPublicFileHelper.UploadSubtitleFilesAsync(
                //     hubContext: _hubContext,
                //     connectionId: connectionId,
                //     user: userExists,
                //     subtitleFiles: createCourseRequestDto.SubtitleFileUploads,
                //     azureBlobStorageService: _azureBlobStorageService,
                //     courseOrLessonObj: course,
                //     logger: _logger,
                //     transaction: _transaction,
                //     courseSubtitles: listCourseSubtitles,
                //     courseRepository: _courseRepository);
                // if (uploadSubtitlesFileResult is not null)
                // {
                //     return BaseReturnHelper<CreateCourseResponseDto>.GenerateErrorResponse(
                //         errorMessage: uploadSubtitlesFileResult.Value.errorMessage,
                //         statusCode: uploadSubtitlesFileResult.Value.statusCode,
                //         message: "Create course failed",
                //         data: null);
                // }
                if (createCourseRequestDto.SubtitleFileUploads is not null
                    && createCourseRequestDto.SubtitleFileUploads.Count > 0)
                {
                    if (!string.IsNullOrEmpty(connectionId) && !string.IsNullOrWhiteSpace(connectionId))
                    {
                        await _hubContext.Clients.Client(connectionId: connectionId).SendAsync(
                            method: HubConstants.ReceiveProgress,
                            arg1: "Upload subtitles ....");
                    }

                    var uploadSubtitles = await _azureBlobStorageService.UploadPublicFilesToAzureBlobStorageAsync(
                        user: userExists,
                        courseId: course.Id.ToString(),
                        lessonId: null,
                        fileThumbnail: null,
                        fileSubtitles: createCourseRequestDto.SubtitleFileUploads
                    );
                    // Add range subtitles course
                    listCourseSubtitles = new List<CourseSubtitle>();
                    foreach (var item in uploadSubtitles)
                    {
                        listCourseSubtitles.Add(item: new CourseSubtitle()
                        {
                            Language = $"{item.DetectSubtitleModel?.Code}-{item.DetectSubtitleModel?.Name}",
                            UrlSubtitle = item.FileUrl,
                            FileName = item.FileName,
                            ContainerName = item.ContainerName,
                            CourseId = course.Id
                        });
                    }
                    try
                    {
                        await _courseRepository.AddRangeCourseSubtitlesAsync(listItem: listCourseSubtitles);
                        if (!string.IsNullOrEmpty(connectionId) && !string.IsNullOrWhiteSpace(connectionId))
                        {
                            await _hubContext.Clients.Client(connectionId: connectionId).SendAsync(
                                method: HubConstants.ReceiveProgress,
                                arg1: "Upload subtitles finished ....");
                        }
                    }
                    catch (Exception ex)
                    {
                        await _transaction.RollbackAsync();
                        return BaseReturnHelper<CreateCourseResponseDto>.GenerateErrorResponse(
                            errorMessage: $"{ex.Message}",
                            statusCode: StatusCodes.Status400BadRequest,
                            message: "Create course failed",
                            data: null
                        );
                    }
                }
                // Upload video intro to azure media service for course
                UploadAzureMediaResponseDto? uploadAzureMediaResponse = null;
                // var uploadVideoResult = await UploadPublicFileHelper.UploadVideoAsync<Course>(
                //     hubContext: _hubContext,
                //     connectionId: connectionId,
                //     user: userExists,
                //     courseOrLessonObj: course,
                //     azureMediaService: _azureMediaService,
                //     videoFileUpload: createCourseRequestDto.VideoFileUpload,
                //     uploadAzureMediaResponse: uploadAzureMediaResponse,
                //     transaction: _transaction,
                //     logger: _logger,
                //     courseRepository: _courseRepository);
                if (createCourseRequestDto.VideoFileUpload is not null)
                {
                    // Get file path upload
                    if (!string.IsNullOrEmpty(connectionId) && !string.IsNullOrWhiteSpace(connectionId))
                    {
                        await _hubContext.Clients
                            .Client(connectionId: connectionId)
                            .SendAsync(method: HubConstants.ReceiveProgress,
                                arg1: "Uploading video demo ....");
                    }

                    UploadAzureMediaRequestDto<Course> uploadAzureMediaRequestDto = new UploadAzureMediaRequestDto<Course>(
                        user: userExists, entity: course);
                    uploadAzureMediaRequestDto.FileToUpload = createCourseRequestDto.VideoFileUpload;
                    try
                    {
                        uploadAzureMediaResponse = await _azureMediaService
                            .UploadMediaWithOfflinePlayReadyAndWidevineProtectionServiceAsync(
                                uploadAzureMediaRequestDto: uploadAzureMediaRequestDto,
                                connectionId: connectionId);
                    }
                    catch (Exception ex)
                    {
                        await _transaction.RollbackAsync();
                        _logger.LogError($"An error occured while upload video file to azure media service. {ex.Message}");
                        return BaseReturnHelper<CreateCourseResponseDto>.GenerateErrorResponse(
                            errorMessage: $"An error occured while upload video file.",
                            statusCode: StatusCodes.Status400BadRequest,
                            message: "Create course failed",
                            data: null
                        );
                    }
                    // If upload video file to azure media service failed.
                    if (uploadAzureMediaResponse is null)
                    {
                        await _transaction.RollbackAsync();
                        return BaseReturnHelper<CreateCourseResponseDto>.GenerateErrorResponse(
                            errorMessage: $"An error occured while upload video file.",
                            statusCode: StatusCodes.Status400BadRequest,
                            message: "Create course failed",
                            data: null
                        );
                    }
                    // Insert Course Streaming Urls
                    CourseUrlStreaming courseUrlStreaming = new CourseUrlStreaming()
                    {
                        CourseId = course.Id,
                        AssetName = uploadAzureMediaRequestDto.OutputAssetName,
                        LocatorName = uploadAzureMediaRequestDto.LocatorName,
                        IdentifierKey = uploadAzureMediaResponse.KeyIdentifier,
                        UrlStreamHlsCsf = uploadAzureMediaResponse.PathStreamingModel.HlsStandardUrl,
                        UrlStreamHlsCmaf = uploadAzureMediaResponse.PathStreamingModel.HlsCmafUrl,
                        UrlStreamDashCsf = uploadAzureMediaResponse.PathStreamingModel.DashStandardUrl,
                        UrlStreamDashCmaf = uploadAzureMediaResponse.PathStreamingModel.DashCmafUrl,
                        UrlSmoothStreaming = uploadAzureMediaResponse.PathStreamingModel.SmoothStreamingUrl,
                        PlayReadyUrlLicenseServer = uploadAzureMediaResponse.PathStreamingModel.PlayReadyUrlLicenseServer,
                        WidevineUrlLicenseServer = uploadAzureMediaResponse.PathStreamingModel.WidevineUrlLicenseServer
                    };
                    try
                    {
                        await _courseRepository.AddCourseUrlStreamingAsync(courseUrlStreaming: courseUrlStreaming);
                    }
                    catch (Exception ex)
                    {
                        await _transaction.RollbackAsync();
                        _logger.LogError($"{ex.Message}");
                        return BaseReturnHelper<CreateCourseResponseDto>.GenerateErrorResponse(
                            errorMessage: $"{ex.Message}",
                            statusCode: StatusCodes.Status400BadRequest,
                            message: "Create course failed",
                            data: null
                        );
                    }
                }

                CreateCourseResponseDto courseResponseDto = new()
                {
                    CourseId = course.Id,
                    CourseName = course.Name,
                    CourseDescription = course.CourseDescription,
                    Price = course.Price,
                    ThumbnailUrl = course.Thumbnail,
                    StreamingDto = new StreamingDto()
                    {
                        KeyIdentifier = uploadAzureMediaResponse?.KeyIdentifier,
                        PlayReadyUrlLicenseServer = uploadAzureMediaResponse?.PathStreamingModel.PlayReadyUrlLicenseServer,
                        WidevineUrlLicenseServer = uploadAzureMediaResponse?.PathStreamingModel.WidevineUrlLicenseServer,
                        Token = uploadAzureMediaResponse?.PathStreamingModel.Token,
                        UrlSmoothStreaming = uploadAzureMediaResponse?.PathStreamingModel.SmoothStreamingUrl,
                        UrlStreamDashCmaf = uploadAzureMediaResponse?.PathStreamingModel.DashCmafUrl,
                        UrlStreamDashCsf = uploadAzureMediaResponse?.PathStreamingModel.DashStandardUrl
                    },
                    SubtitleDtos = listCourseSubtitles is null ? null :
                        listCourseSubtitles.Select(item =>
                            new SubtitleDto() { Language = item.Language, UrlSubtitle = item.UrlSubtitle }).ToList()
                };

                await _transaction.CommitAsync();
                _logger.LogInformation($"Create course with id : {course.Id} sucessfully");
                if (!string.IsNullOrEmpty(connectionId) && !string.IsNullOrWhiteSpace(connectionId))
                {
                    await _hubContext.Clients.Client(connectionId: connectionId)
                        .SendAsync(method: HubConstants.ReceiveProgress, arg1: "Create course successfully.");
                }
                return BaseReturnHelper<CreateCourseResponseDto>.GenerateSuccessResponse(
                    data: courseResponseDto, message: "Create course successfully.");
            };
        }

        public async Task<(int statusCode, BaseResponseWithData<CourseDetailResponseDto> result)> GetCourseDetailServiceAsync(int courseId)
        {
            var userId = GetUserIdFromJwt();
            var ipAddress = GetIpAddress();
            var course = await _courseRepository.GetCourseDetailtAsync(courseId: courseId, userId: userId, ipAddress: ipAddress);
            if (course is null
                || course.CourseUrlStreaming is null
                || course.CourseUrlStreaming.KeyIdentifier is null)
            {
                return BaseReturnHelper<CourseDetailResponseDto>.GenerateErrorResponse(
                    errorMessage: $"Course with id: {courseId} not found.",
                    statusCode: StatusCodes.Status400BadRequest,
                    message: "Get course failed",
                    data: null
                );
            }
            // Set token to play video course demo
            var token = _azureMediaService.GetToken(
                keyIdentifier: course.CourseUrlStreaming.KeyIdentifier
            );
            course.CourseUrlStreaming.Token = token;
            return BaseReturnHelper<CourseDetailResponseDto>.GenerateSuccessResponse(
                data: course,
                message: $"Get course with id : {courseId} successfully.");
        }


        public async Task<(int statusCode, BaseResponseWithData<CourseUpdateResponseDto> result)?>
            UpdateCourseServiceAsync(int courseId, CourseUpdateRequestDto courseUpdateRequestDto)
        {
            var courseExists = await _courseRepository.FindCourseByIdAsync(courseId: courseId);
            if (courseExists is null)
            {
                return BaseReturnHelper<CourseUpdateResponseDto>.GenerateErrorResponse(
                    errorMessage: $"Cannot found any course with id : {courseId}",
                    statusCode: StatusCodes.Status404NotFound,
                    message: "Update course failed",
                    data: null);
            }
            var userId = GetUserIdFromJwt();
            var roles = GetRolesFromJwt();
            if (!string.IsNullOrEmpty(userId)
                && userId != courseExists.UserId
                && roles is not null
                && !roles.Contains(RolesConstant.Admin))
            {
                return BaseReturnHelper<CourseUpdateResponseDto>.GenerateErrorResponse(
                    errorMessage: $"You do not have permission to access this action",
                    statusCode: StatusCodes.Status403Forbidden,
                    message: "Update user failed",
                    data: null);
            }

            using var _transaction = await _courseRepository.CreateTransactionAsync();
            {
                // If thumbnail is not null
                if (courseUpdateRequestDto.ThumbnailFileUpload is not null)
                {
                    try
                    {
                        var userExists = await _courseRepository.FindUserByIdAsync(userId: courseExists.UserId);
                        var uploadInfo = (await _azureBlobStorageService.UploadPublicFilesToAzureBlobStorageAsync(
                            user: userExists!,
                            courseId: courseExists.Id.ToString(),
                            lessonId: null,
                            courseUpdateRequestDto.ThumbnailFileUpload)).FirstOrDefault();
                        if (uploadInfo is not null)
                        {
                            courseExists.BlobContainerName = uploadInfo.ContainerName;
                            courseExists.FileThumbnailName = uploadInfo.FileName;
                            courseExists.Thumbnail = uploadInfo.FileUrl;
                        }
                        else
                        {
                            await _transaction.RollbackAsync();
                            return BaseReturnHelper<CourseUpdateResponseDto>.GenerateErrorResponse(
                                errorMessage: $"An error occured while upload thumbnail file.",
                                statusCode: StatusCodes.Status500InternalServerError,
                                message: "Update course failed",
                                data: null);
                        }
                    }
                    catch (Exception ex)
                    {
                        await _transaction.RollbackAsync();
                        _logger.LogError(ex.Message);
                        return BaseReturnHelper<CourseUpdateResponseDto>.GenerateErrorResponse(
                                errorMessage: $"An error occured while upload thumbnail file.",
                                statusCode: StatusCodes.Status500InternalServerError,
                                message: "Update course failed",
                                data: null);
                    }
                }

                try
                {
                    courseExists.Price = courseUpdateRequestDto.Price;
                    courseExists.Name = courseUpdateRequestDto.Name;
                    courseExists.CourseDescription = courseUpdateRequestDto.CourseDescription;
                    courseExists.IsFree = courseUpdateRequestDto.IsFree;
                    courseExists.IsPublic = courseUpdateRequestDto.IsPublic;
                    await _courseRepository.UpdateCourseAsync(course: courseExists);
                }
                catch (Exception ex)
                {
                    await _transaction.RollbackAsync();
                    _logger.LogError(ex.Message);
                    return BaseReturnHelper<CourseUpdateResponseDto>.GenerateErrorResponse(
                        errorMessage: $"An error occured while update course",
                        statusCode: StatusCodes.Status409Conflict,
                        message: "Update course failed",
                        data: null);
                }
                // update course subtitles
                return null;
            }
        }



        private string? GetUserIdFromJwt()
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var jwtToken = authHeader.Substring("Bearer ".Length).Trim();
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(jwtToken);
                var userId = jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
                return userId;
            }
            return null;
        }

        private List<string>? GetRolesFromJwt()
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var jwtToken = authHeader.Substring("Bearer ".Length).Trim();
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(jwtToken);
                var roles = jwtSecurityToken.Claims.Where(claim => claim.Type == ClaimTypes.Role).Select(item => item.Value).ToList();
                return roles;
            }
            return null;
        }

        private string? GetConnectionIdOfSignalRHub()
            => _httpContextAccessor.HttpContext?.Request.Headers["Connection-Id"].ToString();

        private string? GetIpAddress()
        => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        private string? GetCurrentUserId()
            => _httpContextAccessor?.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

        // /// <summary>
        // /// Delete course subtitles
        // /// </summary>
        // /// <returns></returns>
        // public async Task<(int statusCode, BaseResponseWithData<string> result)> DeleteCourseSubtitles(
        //     int courseId, DeleteCourseSubtitlesDto courseSubtitleIds)
        // {

        // }


        // public async Task UploadThumbnail(string connectionId, AppUser userExists, Course course)
        // {
        //     await _hubContext.Clients.Client(connectionId: connectionId).SendAsync(method: HubConstants.ReceiveProgress, arg1: "Upload thumbnail ....");
        //     UploadPublicFileModel? uploadThumbnailModel = null;
        //     try
        //     {
        //         uploadThumbnailModel = (await _azureBlobStorageService.UploadPublicFilesToAzureBlobStorageAsync(
        //             user: userExists,
        //             courseId: course.Id.ToString(),
        //             lessonId: null,
        //             fileThumbnail: createCourseRequestDto.ThumbnailFileUpload,
        //             fileSubtitles: null
        //         )).FirstOrDefault();
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError($"{ex.Message}");
        //         await _transaction.RollbackAsync();
        //         return BaseReturnHelper<CreateCourseResponseDto>.GenerateErrorResponse(
        //             errorMessage: $"Upload thumbnail failed",
        //             statusCode: StatusCodes.Status500InternalServerError,
        //             message: "Create course failed",
        //             data: null
        //         );
        //     }
        //     if (uploadThumbnailModel is null)
        //     {
        //         await _transaction.RollbackAsync();
        //         return BaseReturnHelper<CreateCourseResponseDto>.GenerateErrorResponse(
        //             errorMessage: $"Upload thumbnail failed",
        //             statusCode: StatusCodes.Status400BadRequest,
        //             message: "Create course failed",
        //             data: null
        //         );
        //     }
        //     // Update thumbnail for course

        //     course.Thumbnail = uploadThumbnailModel.FileUrl;
        //     course.BlobContainerName = uploadThumbnailModel.ContainerName;
        //     course.FileThumbnailName = uploadThumbnailModel.FileName;
        //     try
        //     {
        //         await _courseRepository.UpdateCourseAsync(course: course);
        //         await _hubContext.Clients.Client(connectionId: connectionId).SendAsync(method: HubConstants.ReceiveProgress, arg1: "Upload thumbnail finished.");
        //     }
        //     catch (Exception ex)
        //     {
        //         await _transaction.RollbackAsync();
        //         return BaseReturnHelper<CreateCourseResponseDto>.GenerateErrorResponse(
        //             errorMessage: $"{ex.Message}",
        //             statusCode: StatusCodes.Status400BadRequest,
        //             message: "Create course failed",
        //             data: null
        //         );
        //     }
        // }
    }
}