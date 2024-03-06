using Azure.ResourceManager.Media.Models;
using OnlineCoursePlatform.Configurations;
using OnlineCoursePlatform.DTOs.AzureDtos.Request;
using OnlineCoursePlatform.DTOs.AzureDtos.Response;
using OnlineCoursePlatform.Repositories.AzureRepositories.MediaServiceRepositories;

namespace OnlineCoursePlatform.Services.AzureMediaServices
{
    public class AzureMediaService : IAzureMediaService
    {
        private readonly IAzureMediaServiceRepository _azureMediaServiceRepository;
        private readonly ILogger<AzureMediaService> _logger;
        private readonly IConfiguration _configuration;

        public AzureMediaService(
            IAzureMediaServiceRepository azureMediaServiceRepository,
            ILogger<AzureMediaService> logger,
            IConfiguration configuration)
        {
            _azureMediaServiceRepository = azureMediaServiceRepository;
            _logger = logger;
            _configuration = configuration;
        }


        public async Task<UploadAzureMediaResponseDto?> UploadMediaWithOfflinePlayReadyAndWidevineProtectionServiceAsync<T>(
            UploadAzureMediaRequestDto<T> uploadAzureMediaRequestDto, string connectionId)
        {
            var uploadResponseDto = new UploadAzureMediaResponseDto();
            // Get Azure media account resourse
            var azureMediaAccount = _azureMediaServiceRepository.GetAzureMediaServiceAccountResource();
            if (azureMediaAccount is null)
            {
                _logger.LogError($"An error occured while get azure media service account resource.");
                return null;
            }
            // Create media transform resource
            var transform = await _azureMediaServiceRepository.CreateMediaTransformResourceAsync(
                mediaServicesAccount: azureMediaAccount,
                transformName: uploadAzureMediaRequestDto.TransformName);
            if (transform is null)
            {
                await _azureMediaServiceRepository.CleanUpAsync(
                    transform: transform,
                    job: null,
                    inputAsset: null,
                    outputAsset: null,
                    streamingLocator: null,
                    stopEndpoint: true,
                    streamingEndpoint: null,
                    contentKeyPolicy: null
                );
                _logger.LogError($"An error occured while create media transform resource.");
                return null;
            }
            // Create media input asset resource
            var inputAsset = await _azureMediaServiceRepository.CreateMediaInputAssetResourceAsync(
                mediaServicesAccount: azureMediaAccount,
                assetName: uploadAzureMediaRequestDto.InputAssetName,
                fileToUpload: uploadAzureMediaRequestDto.FileToUpload,
                connectionId: connectionId);
            if (inputAsset is null)
            {
                await _azureMediaServiceRepository.CleanUpAsync(
                    transform: transform,
                    job: null,
                    inputAsset: inputAsset,
                    outputAsset: null,
                    streamingLocator: null,
                    stopEndpoint: true,
                    streamingEndpoint: null,
                    contentKeyPolicy: null
                );
                _logger.LogError($"An error occured while create media input asset resource.");
                return null;
            }
            // Create media output asset resource
            var outputAsset = await _azureMediaServiceRepository.CreateMediaOutputAssetResourceAsync(
                mediaServicesAccount: azureMediaAccount,
                assetName: uploadAzureMediaRequestDto.OutputAssetName);
            if (outputAsset is null)
            {
                await _azureMediaServiceRepository.CleanUpAsync(
                    transform: transform,
                    job: null,
                    inputAsset: inputAsset,
                    outputAsset: outputAsset,
                    streamingLocator: null,
                    stopEndpoint: true,
                    streamingEndpoint: null,
                    contentKeyPolicy: null
                );
                _logger.LogError($"An error occured while create media output asset resource.");
                return null;
            }
            // Create media job resource
            var job = await _azureMediaServiceRepository.SubmitJobAsync(
                transform: transform,
                jobName: uploadAzureMediaRequestDto.JobName,
                inputAsset: inputAsset,
                outputAsset: outputAsset,
                connectionId: connectionId);
            if (job is null)
            {
                await _azureMediaServiceRepository.CleanUpAsync(
                    transform: transform,
                    job: job,
                    inputAsset: inputAsset,
                    outputAsset: outputAsset,
                    streamingLocator: null,
                    stopEndpoint: true,
                    streamingEndpoint: null,
                    contentKeyPolicy: null
                );
                _logger.LogError($"An error occured while create media job resource.");
                return null;
            }
            // Wait for job to finish
            job = await _azureMediaServiceRepository.WaitForJobToFinishAsync(job: job, connectionId: connectionId);

            // If job failed
            if (job.Data.State == MediaJobState.Error)
            {
                // Clean all media to upload file
                await _azureMediaServiceRepository.CleanUpAsync(
                    transform: transform,
                    job: job,
                    inputAsset: inputAsset,
                    outputAsset: outputAsset,
                    streamingLocator: null,
                    stopEndpoint: true,
                    streamingEndpoint: null,
                    contentKeyPolicy: null);
                _logger.LogError($"An error occured while create job.");
                return null;
            }
            // Get content key policy symmetric token key
            ContentKeyPolicySymmetricTokenKey ckTokenSigningKey = _azureMediaServiceRepository.GetContentKeyPolicySymmetricTokenKey();
            // Create content key policy resource
            var contentKeyPolicy = await _azureMediaServiceRepository
                .CreateContentKeyPolicyResourceOfflinePlayReadyAndWidevineAsync(
                    mediaServicesAccountResource: azureMediaAccount,
                    contentKeyPolicyName: uploadAzureMediaRequestDto.ContentKeyPolicyName,
                    ckTokenSigningKey: ckTokenSigningKey,
                    connectionId: connectionId);
            // Create streaming locator resource
            var streamingLocator = await _azureMediaServiceRepository.CreateStreamingLocatorResourceAsync(
                mediaServicesAccount: azureMediaAccount,
                asset: outputAsset,
                locatorName: uploadAzureMediaRequestDto.LocatorName,
                contentPolicyName: contentKeyPolicy.Data.Name,
                streamingPolicyName: "Predefined_MultiDrmCencStreaming",
                connectionId: connectionId
            );
            // Save ContentKeyPolicy Identifier
            var keyIdentifier = streamingLocator.Data.ContentKeys.FirstOrDefault()?.Id.ToString();
            if (keyIdentifier is not null)
            {
                uploadResponseDto.KeyIdentifier = keyIdentifier;
            }
            // Get token for streaming url
            string token = _azureMediaServiceRepository.GetToken(
                keyIdentifier: uploadResponseDto.KeyIdentifier,
                ckTokenKey: ckTokenSigningKey);
            // Get streaming endpoint resource
            var streamingEndpoint = await _azureMediaServiceRepository.GetStreamingEndpointResourceAsync(
                mediaServicesAccount: azureMediaAccount);
            // If streaming endpoint resource not running
            await _azureMediaServiceRepository.StartStreamingEndpointResourceAsync(streamingEndpoint: streamingEndpoint);
            // Get Url streaming Hls, Dash, Smooth Streaming and Download Urls
            var pathsModel = await _azureMediaServiceRepository.GetUrlStreamingAsync(
                locator: streamingLocator,
                streamingEndpoint: streamingEndpoint);
            uploadResponseDto.PathStreamingModel.DashCmafUrl = pathsModel.DashCmafUrl;
            uploadResponseDto.PathStreamingModel.DashStandardUrl = pathsModel.DashStandardUrl;
            uploadResponseDto.PathStreamingModel.SmoothStreamingUrl = pathsModel.SmoothStreamingUrl;
            // Save token for streaming url
            uploadResponseDto.PathStreamingModel.Token = token;
            // Create Urls License Server for streaming video to authorized from client
            var mediaAccount = _configuration[AppSettingsConfig.AZURE_MEDIA_SERVICES_ACCOUNT_NAME];
            var location = _configuration[AppSettingsConfig.AZURE_LOCATION];
            uploadResponseDto.PathStreamingModel.PlayReadyUrlLicenseServer = $"https://{mediaAccount}.keydelivery.{location}.media.azure.net/PlayReady/?kid={keyIdentifier}";
            uploadResponseDto.PathStreamingModel.WidevineUrlLicenseServer = $"https://{mediaAccount}.keydelivery.{location}.media.azure.net/Widevine/?kid={keyIdentifier}";
            // return model info media uploaded
            return uploadResponseDto;
        }

        public string GetToken(string keyIdentifier)
            => _azureMediaServiceRepository.GetToken(
                keyIdentifier: keyIdentifier,
                ckTokenKey: _azureMediaServiceRepository.GetContentKeyPolicySymmetricTokenKey());
    }
}