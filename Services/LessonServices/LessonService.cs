using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineCoursePlatform.Base.BaseResponse;
using OnlineCoursePlatform.Data.DbContext;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.Data.Entities.Order;
using OnlineCoursePlatform.DTOs.AzureDtos.Request;
using OnlineCoursePlatform.DTOs.LessonDtos;
using OnlineCoursePlatform.Helpers;
using OnlineCoursePlatform.Services.AzureBlobStorageServices;
using OnlineCoursePlatform.Services.AzureMediaServices;

namespace OnlineCoursePlatform.Services.LessonServices
{
    public class LessonService : ILessonService
    {
        private readonly OnlineCoursePlatformDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<LessonService> _logger;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly IAzureBlobStorageService _azureBlob;
        private readonly IAzureMediaService _azureMedia;

        public LessonService(
            OnlineCoursePlatformDbContext dbContext,
            UserManager<AppUser> userManager,
            ILogger<LessonService> logger,
            IHttpContextAccessor httpAccessor,
            IAzureBlobStorageService azureBlob,
            IAzureMediaService azureMedia)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _logger = logger;
            _azureBlob = azureBlob;
            _azureMedia = azureMedia;
            _httpAccessor = httpAccessor;
        }

        public async Task<(int statusCode, BaseResponseWithData<List<LessonResponseDto>> data)> 
            GetLessonsOfCourseAsync(GetLessonsOfCourseRequestDto requestDto)
        {
            // 
            var currentCourse = await _dbContext.Courses.FindAsync(requestDto.CourseId);
            if (currentCourse == null)
            {
                return BaseReturnHelper<List<LessonResponseDto>>.GenerateErrorResponse(
                    errorMessage: $"The course with id : {requestDto.CourseId} not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    message: $"Get lessons of course failed",
                    data: null);
            }
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return BaseReturnHelper<List<LessonResponseDto>>.GenerateErrorResponse(
                    errorMessage: $"Invalid authentication.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: $"Get lessons of course failed",
                    data: null);
            }
            // check if current user have permission watch lesson of course
            var courseOrdered = await _dbContext.OrderDetails
                .Include(od => od.OrderCourse)
                .Include(od => od.OrderCourse)
                .Where(od => od.CourseId == currentCourse.Id
                    && od.OrderCourse.UserId == currentCourse.UserId
                    && od.OrderCourse.Status == OrderStatus.Success
                    && od.OrderDate <= DateTime.UtcNow 
                    && od.ExpireDate >= DateTime.UtcNow)
                .FirstOrDefaultAsync();
            if (courseOrdered == null && !currentCourse.IsPublic)
            {
                return BaseReturnHelper<List<LessonResponseDto>>.GenerateErrorResponse(
                    errorMessage: $"You don't have permission access to this course.",
                    statusCode: StatusCodes.Status403Forbidden,
                    message: $"Get lessons of course failed",
                    data: null);
            }
            // get lesson of course
            var lessonsOfCourse = await _dbContext.Lessons
                .Where(l => l.CourseId == currentCourse.Id)
                .Select(l => new LessonResponseDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    LessonIndex = l.LessonIndex,
                    Thumbnail = l.Thumbnail,
                    IsPublic = courseOrdered == null ? l.IsPublic : true
                })
                .OrderBy(l => l.LessonIndex)
                .ToListAsync();
            return BaseReturnHelper<List<LessonResponseDto>>.GenerateSuccessResponse(
                data: lessonsOfCourse,
                message: "Get lessons of course successfully"
            );
        }

        public async Task<(int statusCode , BaseResponseWithData<LessonDetailResponseDto> result)>
            GetLessonDetailAsync(GetLessonDetailRequestDto requestDto)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return BaseReturnHelper<LessonDetailResponseDto>.GenerateErrorResponse(
                    errorMessage: "Authentication failed",
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Get lesson detail failed",
                    data: null);
            }
            var currentLesson = await _dbContext.Lessons
                .Include(l => l.LessonSubtitles)
                .Include(l => l.LessonUrlStreamings)
                .Where(l => l.Id == requestDto.LessonId)
                .FirstOrDefaultAsync();
            if (currentLesson == null)
            {
                return BaseReturnHelper<LessonDetailResponseDto>.GenerateErrorResponse(
                    errorMessage: $"Lesson with id {requestDto.LessonId} not found",
                    statusCode: StatusCodes.Status404NotFound,
                    message: "Get lesson detail failed",
                    data: null);
            }
            // check current user have permission access to this course
            var currentCourse = await _dbContext.Courses.FindAsync(currentLesson.CourseId);
            if (currentCourse == null)
            {
                return BaseReturnHelper<LessonDetailResponseDto>.GenerateErrorResponse(
                    errorMessage: $"Course of lesson have id {requestDto.LessonId} not found",
                    statusCode: StatusCodes.Status404NotFound,
                    message: "Get lesson detail failed",
                    data: null);
            }
            var courseOrdered = await _dbContext.OrderDetails
                .Include(od => od.Course)
                .Include(od => od.OrderCourse)
                .Where(od => od.CourseId == currentCourse.Id
                    && od.OrderCourse.UserId == currentCourse.UserId
                    && od.OrderCourse.Status == OrderStatus.Success
                    && od.OrderDate <= DateTime.UtcNow 
                    && od.ExpireDate >= DateTime.UtcNow)
                .FirstOrDefaultAsync();
            if (courseOrdered == null && !currentCourse.IsPublic 
                && currentCourse.UserId != currentUser.Id)
            {
                return BaseReturnHelper<LessonDetailResponseDto>.GenerateErrorResponse(
                    errorMessage: $"You don't have permission access to this lesson.",
                    statusCode: StatusCodes.Status403Forbidden,
                    message: $"Get lessons of course failed",
                    data: null);
            }
            if (courseOrdered != null && courseOrdered.ExpireDate < DateTime.UtcNow)
            {
                return BaseReturnHelper<LessonDetailResponseDto>.GenerateErrorResponse(
                    errorMessage: $"The course has expired",
                    statusCode: StatusCodes.Status403Forbidden,
                    message: $"Get lessons of course failed",
                    data: null);
            }
            // get lesson detail
            LessonUrlStreaming? lessonStreaming = currentLesson.LessonUrlStreamings.FirstOrDefault();
            var lessonSubtitles = currentLesson.LessonSubtitles.ToList();

            var lessonDetail = new LessonDetailResponseDto()
            {
                LessonStreamingProperty = lessonStreaming == null ? new LessonStreamingProperty() : new LessonStreamingProperty()
                {
                    PlayReadyUrlLicenseServer = lessonStreaming.PlayReadyUrlLicenseServer,
                    WidevineUrlLicenseServer = lessonStreaming.WidevineUrlLicenseServer,
                    UrlSmoothStreaming = lessonStreaming.UrlSmoothStreaming,
                    UrlStreamDashCmaf = lessonStreaming.UrlStreamDashCmaf,
                    UrlStreamDashCsf = lessonStreaming.UrlStreamDashCsf,
                    UrlStreamHlsCmaf = lessonStreaming.UrlStreamHlsCmaf,
                    UrlStreamHlsCsf = lessonStreaming.UrlStreamHlsCsf,
                    Token = string.IsNullOrEmpty(lessonStreaming.IdentifierKey) ? string.Empty : _azureMedia.GetToken(
                        keyIdentifier: lessonStreaming.IdentifierKey)
                },
                LessonSubtitleProperties = lessonSubtitles.Select(ls => new LessonSubtitleProperty()
                {
                    Language = ls.Language,
                    SubtitleUrl = ls.UrlSubtitle
                }).ToList()
            };

            return BaseReturnHelper<LessonDetailResponseDto>.GenerateSuccessResponse(
                data: lessonDetail,
                message: "Get lesson detail successfully");
        }

        public async Task<AddLessResponseDto?> AddLessonAsync(AddLessonRequestDto requestDto)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return null;
            }
            if (currentUser.Id != _dbContext.Courses.Find(requestDto.CourseId)?.UserId)
            {
                return null;
            }
            
            Lesson? lesson = new Lesson()
            {
                Name = requestDto.Name,
                Description = requestDto.Description,
                CourseId = requestDto.CourseId,
                DateRelease = requestDto.DateRealease,
                IsPublic = requestDto.IsPublic,
                // UploadCost = requestDto.VideoFile.
            };
            var transaction = await _dbContext.Database.BeginTransactionAsync();
            {
                // if thumbnail file not null
                _dbContext.Lessons.Add(lesson);
                try
                {
                    await _dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    await transaction.RollbackAsync();
                    return null;
                }
                if (requestDto.ThumbnailFile != null)
                {
                    var result = (await _azureBlob.UploadPublicFilesToAzureBlobStorageAsync(
                        user: currentUser,
                        courseId: null,
                        lessonId: lesson.Id.ToString(),
                        fileThumbnail: requestDto.ThumbnailFile)).FirstOrDefault();
                    lesson.Thumbnail = result?.FileUrl;
                    lesson.BlobContainerName = result?.ContainerName;
                    lesson.ThumbnailName = result?.FileName;
                    _dbContext.Lessons.Update(lesson);
                    try
                    {
                        await _dbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                        await transaction.RollbackAsync();
                        return null;
                    }
                }
                // upload subtitles
                if (!requestDto.SubtitleFiles.IsNullOrEmpty())
                {
                    var result = await _azureBlob.UploadPublicFilesToAzureBlobStorageAsync(
                        user: currentUser,
                        courseId: null,
                        lessonId: lesson.Id.ToString(),
                        fileSubtitles: requestDto.SubtitleFiles);
                    var lessonSubtitles = new List<LessonSubtitle>();
                    result.ForEach(r => lessonSubtitles.Add(new LessonSubtitle()
                    {
                        ContainerName = r.ContainerName,
                        FileName = r.FileName,
                        UrlSubtitle = r.FileUrl,
                        LessonId = lesson.Id,
                        Language = $"{r.DetectSubtitleModel?.Code} - {r.DetectSubtitleModel?.Name}"
                    }));

                    _dbContext.LessonSubtitles.AddRange(entities: lessonSubtitles);
                    try
                    {
                        await _dbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                        await transaction.RollbackAsync();
                        return null;
                    }
                }

                // upload video
                UploadAzureMediaRequestDto<Lesson> uploadRequestDto = new UploadAzureMediaRequestDto<Lesson>(
                        user: currentUser, entity: lesson);
                uploadRequestDto.FileToUpload = requestDto.VideoFile;
                if (requestDto.VideoFile != null)
                {
                    var result = await _azureMedia.UploadMediaWithOfflinePlayReadyAndWidevineProtectionServiceAsync(
                        uploadAzureMediaRequestDto: uploadRequestDto,
                        connectionId: "");
                    var lessonUrlStreaming = new LessonUrlStreaming()
                    {
                        AssetName = uploadRequestDto.OutputAssetName,
                        LessonId = lesson.Id,
                        IdentifierKey = result?.KeyIdentifier,
                        LocatorName = uploadRequestDto.LocatorName,
                        SigningTokenKey = uploadRequestDto.ContentKeyPolicyName,
                        PlayReadyUrlLicenseServer = result?.PathStreamingModel?.PlayReadyUrlLicenseServer,
                        WidevineUrlLicenseServer = result?.PathStreamingModel?.WidevineUrlLicenseServer,
                        UrlSmoothStreaming = result?.PathStreamingModel?.SmoothStreamingUrl,
                        UrlStreamDashCmaf = result?.PathStreamingModel?.DashCmafUrl,
                        UrlStreamDashCsf = result?.PathStreamingModel?.DashStandardUrl,
                        UrlStreamHlsCmaf = result?.PathStreamingModel?.HlsCmafUrl,
                        UrlStreamHlsCsf = result?.PathStreamingModel?.HlsStandardUrl
                    };
                    _dbContext.LessonUrlStreamings.Add(lessonUrlStreaming);
                    try
                    {
                        await _dbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                        await transaction.RollbackAsync();
                        return null;
                    }
                }
                await transaction.CommitAsync();
            }
            return new AddLessResponseDto()
            {
                Id = lesson.Id,
                Name = lesson.Name,
                LessonIndex = lesson.LessonIndex,
                Description = lesson.Description
            };
        }




        public async Task<AppUser?> GetCurrentUserAsync()
        {
            ClaimsPrincipal? currentClaimsPrincipal = _httpAccessor.HttpContext?.User;
            if (currentClaimsPrincipal != null)
            {
                return await _userManager.GetUserAsync(principal: currentClaimsPrincipal);
            }
            return null;
        }
    }
}