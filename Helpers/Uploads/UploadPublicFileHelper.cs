using System.Data;
using Mapster;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Storage;
using OnlineCoursePlatform.Constants;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.DTOs.AzureDtos.Request;
using OnlineCoursePlatform.DTOs.AzureDtos.Response;
using OnlineCoursePlatform.DTOs.CourseDtos;
using OnlineCoursePlatform.Hubs;
using OnlineCoursePlatform.Models.UploadFileModels;
using OnlineCoursePlatform.Repositories.CourseRepositories;
using OnlineCoursePlatform.Repositories.LessonRepositories;
using OnlineCoursePlatform.Services.AzureBlobStorageServices;
using OnlineCoursePlatform.Services.AzureMediaServices;

namespace OnlineCoursePlatform.Helpers.Uploads
{
    public class UploadPublicFileHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hubContext"></param>
        /// <param name="connectionId"></param>
        /// <param name="user"></param>
        /// <param name="azureBlobStorageService"></param>
        /// <param name="courseOrLessonObj"></param>
        /// <param name="fileToUpload"></param>
        /// <param name="transaction"></param>
        /// <param name="logger"></param>
        /// <param name="courseRepository"></param>
        /// <param name="lessonRepository"></param>
        /// <returns>If return null -> upload successfully</returns>
        public static async Task<(int statusCode, string errorMessage)?> UploadThumbnailFileAsync(
            IHubContext<ProgressHub> hubContext,
            string connectionId,
            AppUser user,
            IAzureBlobStorageService azureBlobStorageService,
            object courseOrLessonObj,
            IFormFile? fileToUpload,
            IDbContextTransaction transaction,
            ILogger logger,
            ICourseRepository? courseRepository = null,
            ILessonRepository? lessonRepository = null)
        {
            if (fileToUpload is not null)
            {
                await SendProgressSignalRMessage(
                    hubContext: hubContext,
                    message: "Start uploading thumbnail...",
                    connectionId: connectionId);

                bool isCourse = courseOrLessonObj is Course;
                dynamic courseOrLesson = isCourse ? (Course)courseOrLessonObj : (Lesson)courseOrLessonObj;

                UploadPublicFileModel? uploadThumbnailModel = null;
                try
                {
                    string? courseId = null;
                    string? lessonId = null;
                    if (isCourse)
                    {
                        courseId = (courseOrLessonObj as Course)?.Id.ToString();
                    }
                    else
                    {
                        lessonId = (courseOrLessonObj as Lesson)?.Id.ToString();
                    }
                    
                    uploadThumbnailModel = (await azureBlobStorageService.UploadPublicFilesToAzureBlobStorageAsync(
                        user: user,
                        courseId: courseId,
                        lessonId: lessonId,
                        fileThumbnail: fileToUpload,
                        fileSubtitles: null
                    )).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    logger.LogError($"{ex.Message}");
                    await transaction.RollbackAsync();
                    return (statusCode: StatusCodes.Status500InternalServerError,
                        errorMessage: "An error occured while uploading thumbnail");
                }
                if (uploadThumbnailModel is null)
                {
                    await transaction.RollbackAsync();

                    return (statusCode: StatusCodes.Status400BadRequest,
                        errorMessage: "Upload thumbnail failed");
                }
                // update thumbnail
                courseOrLesson.Thumbnail = uploadThumbnailModel.FileUrl;
                courseOrLesson.BlobContainerName = uploadThumbnailModel.ContainerName;
                courseOrLesson.FileThumbnailName = uploadThumbnailModel.FileName;
                try
                {
                    if (isCourse)
                    {
                        var course = courseOrLessonObj as Course;
                        if (course is not null)
                            await courseRepository!.UpdateCourseAsync(course: course);
                    }
                    else
                    {
                        var lesson = courseOrLessonObj as Lesson;
                        if (lesson is not null)
                            await lessonRepository!.UpdateLessonAsync(lesson: lesson);
                    }
                    await SendProgressSignalRMessage(hubContext: hubContext,
                            message: "Upload thumbnail finished.",
                            connectionId: connectionId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);
                    await transaction.RollbackAsync();
                    return (statusCode: StatusCodes.Status500InternalServerError,
                        errorMessage: "An error ocurred while upload entity");
                }
            }
            return null;
        }


        public static async Task<(int statusCode, string errorMessage)?> UploadSubtitleFilesAsync(
            IHubContext<ProgressHub> hubContext,
            string connectionId,
            AppUser user,
            List<IFormFile>? subtitleFiles,
            IAzureBlobStorageService azureBlobStorageService,
            object courseOrLessonObj,
            ILogger logger,
            IDbContextTransaction transaction,
            List<CourseSubtitle>? courseSubtitles = null,
            List<LessonSubtitle>? lessonSubtitles = null,
            ICourseRepository? courseRepository = null,
            ILessonRepository? lessonRepository = null)
        {
            if (subtitleFiles is not null
                && subtitleFiles.Count > 0)
            {
                await hubContext.Clients.Client(connectionId: connectionId).SendAsync(
                    method: HubConstants.ReceiveProgress,
                    arg1: "Upload subtitles ....");

                bool isCourse = courseOrLessonObj is Course;
                dynamic courseOrLesson = isCourse ? (Course)courseOrLessonObj : (Lesson)courseOrLessonObj;

                var uploadSubtitles = await azureBlobStorageService.UploadPublicFilesToAzureBlobStorageAsync(
                    user: user,
                    courseId: isCourse ? courseOrLesson.Id.ToString() : null,
                    lessonId: isCourse ? null : courseOrLesson.Id.ToString(),
                    fileThumbnail: null,
                    fileSubtitles: subtitleFiles
                );
                // Add range subtitles course
                try
                {
                    if (isCourse)
                    {
                        courseSubtitles = new List<CourseSubtitle>();
                        foreach (var item in uploadSubtitles)
                        {
                            courseSubtitles.Add(item: new CourseSubtitle()
                            {
                                Language = $"{item.DetectSubtitleModel?.Code}-{item.DetectSubtitleModel?.Name}",
                                UrlSubtitle = item.FileUrl,
                                FileName = item.FileName,
                                ContainerName = item.ContainerName,
                                CourseId = courseOrLesson.Id
                            });
                        }
                        await courseRepository!.AddRangeCourseSubtitlesAsync(listItem: courseSubtitles);
                    }
                    else
                    {
                        lessonSubtitles = new List<LessonSubtitle>();
                        foreach (var item in uploadSubtitles)
                        {
                            lessonSubtitles.Add(item: new LessonSubtitle()
                            {
                                Language = $"{item.DetectSubtitleModel?.Code}-{item.DetectSubtitleModel?.Name}",
                                UrlSubtitle = item.FileUrl,
                                FileName = item.FileName,
                                ContainerName = item.ContainerName,
                                LessonId = courseOrLesson.Id
                            });
                        }
                        await lessonRepository!.AddRangeLessonSubtitlesAsync(listItem: lessonSubtitles);
                    }
                    await hubContext.Clients.Client(connectionId: connectionId).SendAsync(
                            method: HubConstants.ReceiveProgress,
                            arg1: "Upload subtitles finished ....");
                }
                catch (Exception ex)
                {
                    logger.LogError($"{ex.Message}");
                    await transaction.RollbackAsync();
                    return (statusCode: StatusCodes.Status500InternalServerError,
                        errorMessage: "An error ocurred while upload subtitle files.");
                }
                logger.LogInformation("Upload subtite sucessfully.");
                return null;
            }
            return null;
        }


        public static async Task<(int statusCode, string errorMessage)?> UploadVideoAsync<T>(
            IHubContext<ProgressHub> hubContext,
            string connectionId,
            AppUser user,
            T courseOrLessonObj,
            IAzureMediaService azureMediaService,
            IFormFile? videoFileUpload,
            UploadAzureMediaResponseDto? uploadAzureMediaResponse,
            IDbContextTransaction transaction,
            ILogger logger,
            ICourseRepository? courseRepository = null,
            ILessonRepository? lessonRepository = null)
        {
            if (videoFileUpload is not null)
            {
                // Get file path upload
                await hubContext.Clients
                    .Client(connectionId: connectionId)
                    .SendAsync(method: HubConstants.ReceiveProgress,
                        arg1: "Uploading video ....");
                // bool isCourse = courseOrLessonObj is Course;
                // dynamic courseOrLesson = isCourse ? (Course)courseOrLessonObj : (Lesson)courseOrLessonObj;

                var uploadAzureMediaRequestDto = new UploadAzureMediaRequestDto<T>(
                    user: user, entity: courseOrLessonObj);
                uploadAzureMediaRequestDto.FileToUpload = videoFileUpload;
                try
                {
                    uploadAzureMediaResponse = await azureMediaService
                        .UploadMediaWithOfflinePlayReadyAndWidevineProtectionServiceAsync(
                            uploadAzureMediaRequestDto: uploadAzureMediaRequestDto,
                            connectionId: connectionId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    logger.LogError($"An error occured while upload video file to azure media service. {ex.Message}");
                    return (statusCode: StatusCodes.Status500InternalServerError,
                        errorMessage: "An error occured while upload video file.");
                }
                // If upload video file to azure media service failed.
                if (uploadAzureMediaResponse is null)
                {
                    await transaction.RollbackAsync();
                    return (statusCode: StatusCodes.Status400BadRequest,
                        errorMessage: "An error occured while upload video file.");
                }
                // Insert Course Streaming Urls
                BaseUploadVideo baseUploadVideo = new BaseUploadVideo()
                {
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
                if (courseOrLessonObj is Course)
                {
                    Course? course = courseOrLessonObj as Course;
                    var courseUrlStreaming = baseUploadVideo.Adapt<CourseUrlStreaming>();
                    if (course is not null)
                    {
                        courseUrlStreaming.Id = course.Id;
                    }
                    try
                    {
                        await courseRepository!.AddCourseUrlStreamingAsync(courseUrlStreaming: courseUrlStreaming);
                        return null;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        logger.LogError($"{ex.Message}");
                        return (statusCode: StatusCodes.Status500InternalServerError,
                            errorMessage: "An error ocurred while create url streaming.");
                    }
                }
                else
                {
                    Lesson? lesson = courseOrLessonObj as Lesson;
                    var lessonUrlStreaming = baseUploadVideo.Adapt<LessonUrlStreaming>();
                    if (lesson is not null)
                    {
                        lessonUrlStreaming.Id = lesson.Id;
                    }
                    try
                    {
                        await lessonRepository!.AddLessonUrlStreamingAsync(lessonUrlStreaming: lessonUrlStreaming);
                        return null;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        logger.LogError($"{ex.Message}");
                        return (statusCode: StatusCodes.Status500InternalServerError,
                            errorMessage: "An error ocurred while create url streaming.");
                    }
                }
            }
            return null;
        }


        public static async Task SendProgressSignalRMessage(
            IHubContext<ProgressHub> hubContext,
            string message,
            string connectionId)
        => await hubContext.Clients
                .Client(connectionId: connectionId)
                .SendAsync(method: HubConstants.ReceiveProgress, arg1: message);


    }
}