using Azure.Storage.Blobs;
using OnlineCoursePlatform.Data.DbContext;

namespace OnlineCoursePlatform.Repositories.AzureRepositories.BlobStorageRepositories
{
    public class BlobStorageRepository : IBlobStorageRepository
    {
        private readonly OnlineCoursePlatformDbContext _dbContext;
        private readonly ILogger<BlobStorageRepository> _logger;
        private readonly BlobServiceClient _blobServiceClient;

        public BlobStorageRepository(
            OnlineCoursePlatformDbContext dbContext,
            ILogger<BlobStorageRepository> logger,
            BlobServiceClient blobServiceClient)
        {
            _dbContext = dbContext;
            _logger = logger;
            _blobServiceClient = blobServiceClient;
        }

        public async Task GetBlobContainerOfUserAsync(string blobContainerName, string blobFileName)
        {
            try
            {
                var blobContainer = _blobServiceClient.GetBlobContainerClient(blobContainerName: blobContainerName);
                var blobFile = blobContainer.GetBlobClient(blobName: blobFileName);
                await blobFile.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occured while delete blob file client.\n Trace: {ex.Message}");
                throw new Exception($"An error occured while delete blob file client.");
            }
        }

        // public async Task<DownloadVideoInfo> GetDownloadVideoInfoAsync()
    }
}