using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DetectLanguage;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.Data.Entities.Chat;
using OnlineCoursePlatform.DTOs.FileUploadDtos.Request;
using OnlineCoursePlatform.Models.SubtitleModels;
using OnlineCoursePlatform.Models.UploadFileModels;

namespace OnlineCoursePlatform.Services.AzureBlobStorageServices
{
    public class AzureBlobStorageService : IAzureBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        public AzureBlobStorageService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<List<UploadPublicFileModel>> UploadPublicFilesToAzureBlobStorageAsync(
            AppUser user,
            string? courseId,
            string? lessonId,
            IFormFile? fileThumbnail = null,
            List<IFormFile>? fileSubtitles = null)
        {
            var uploadPublicFileModels = new List<UploadPublicFileModel>();
            // Get blob container of user

            // Get blob container if not exists then create it.
            var containerClient = _blobServiceClient.GetBlobContainerClient($"container-{user.Email?.Split('@')[0]}");
            await containerClient.CreateIfNotExistsAsync();
            // Set public permission for blob
            await containerClient.SetAccessPolicyAsync(accessType: PublicAccessType.Blob);

            // If a single file is provided, add it to the list
            if (fileThumbnail != null)
            {
                if (fileSubtitles == null)
                {
                    fileSubtitles = new List<IFormFile>();
                }
                fileSubtitles.Add(fileThumbnail);
            }

            // If no files are provided, return an empty list
            if (fileSubtitles == null)
            {
                return uploadPublicFileModels;
            }

            foreach (var file in fileSubtitles)
            {
                // Determine the type of the file
                string fileType = file.ContentType.Split('/')[0];

                // Choose the folder based on the file type
                string folder = (fileType == "image") ? "Images" : "Subtitles";

                // Detect language if file is subtitle
                string fileName = $"CourseId-{courseId}" + (lessonId != null ? $"-LessonId-{lessonId}" : "") + "-image-thumbnail";
                DetectSubtitleModel? detectSubtitleModel = new();
                if (folder == "Subtitles")
                {
                    // Get language of subtitle
                    detectSubtitleModel = await DetectSubtitleLanguage(file: file);
                    fileName = $"CourseId-{courseId}" + (lessonId != null ? $"-LessonId-{lessonId}" : "") + $"-subtitle-{detectSubtitleModel?.Code}";
                }

                string folderName = $"CourseId-{courseId}" + (lessonId != null ? $"/LessonId-{lessonId}" : "");
                string blobName = $"{folderName}/{folder}/{fileName}{Path.GetExtension(file.FileName)}";
                var blobClient = containerClient.GetBlobClient(blobName: blobName);
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(target: stream);
                    stream.Position = 0;
                    var blobUploadOptions = new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = file.ContentType,
                        }
                    };
                    await blobClient.UploadAsync(content: stream, options: blobUploadOptions);
                }
                uploadPublicFileModels.Add(item: new UploadPublicFileModel()
                {
                    ContainerName = blobClient.BlobContainerName,
                    FileName = blobClient.Name,
                    FileUrl = blobClient.Uri.ToString(),
                    DetectSubtitleModel = detectSubtitleModel
                });
            }

            return uploadPublicFileModels;
        }

        public async Task<List<UploadChatFileModel>> UploadChatFiles(
            UploadChatFilesRequestDto requestDto,
            AppUser user)
        {
            var uploadChatsFileModel = new List<UploadChatFileModel>();
            // Get blob container of user

            // Get blob container if not exists then create it.
            var containerClient = _blobServiceClient.GetBlobContainerClient($"container-conversation-chat-{requestDto.GroupChatId}");
            await containerClient.CreateIfNotExistsAsync();
            // Set public permission for blob
            await containerClient.SetAccessPolicyAsync(accessType: PublicAccessType.Blob);

            // If no files are provided, return an empty list
            if (requestDto.Files == null)
            {
                return uploadChatsFileModel;
            }

            foreach (var file in requestDto.Files)
            {
                // Determine the type of the file
                string blobName = $"{Path.GetExtension(file.FileName)}{DateTime.Now}";
                var blobClient = containerClient.GetBlobClient(blobName: blobName);
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(target: stream);
                    stream.Position = 0;
                    var blobUploadOptions = new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = file.ContentType,
                        }
                    };
                    await blobClient.UploadAsync(content: stream, options: blobUploadOptions);
                }
                uploadChatsFileModel.Add(item: new UploadChatFileModel()
                {
                    BlobContainerName = blobClient.BlobContainerName,
                    FileName = blobClient.Name,
                    FileUrl = blobClient.Uri.ToString(),
                    FileType = GetFileType(file: file)
                });
            }
            return uploadChatsFileModel;
        }


        public async Task DeleteFileFromAzureBlobStorageAsync(
            AppUser user,
            string fileName,
            string folder)
        {
            // Get blob container
            var containerClient = _blobServiceClient.GetBlobContainerClient($"container-{user.Email?.Split('@')[0]}");

            // Get blob client
            var blobClient = containerClient.GetBlobClient(blobName: $"{folder}/{fileName}");

            // Delete the blob
            await blobClient.DeleteIfExistsAsync();
        }


        private async Task<DetectSubtitleModel?> DetectSubtitleLanguage(IFormFile file)
        {
            string LanguageDetectionAPIKey = @"a5a42d1e837df4f9291614e61dd649b8";
            DetectLanguageClient client = new DetectLanguageClient(LanguageDetectionAPIKey);
            // Read the content of the file
            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();

            // Use the DetectLanguage client to detect the language
            string languageCode = await client.DetectCodeAsync(content);
            var languages = await client.GetLanguagesAsync();
            return languages.Select(item =>
                new DetectSubtitleModel { Code = item.code, Name = item.name })
                    .FirstOrDefault(item => item.Code == languageCode);
        }


        public FileType GetFileType(IFormFile file)
        {
            var contentType = file.ContentType;
            switch (contentType)
            {
                case var type when type.StartsWith("image"):
                    return FileType.Image;
                case var type when type.StartsWith("video"):
                    return FileType.Video;
                default:
                    return FileType.Other;
            }
        }
    }
}