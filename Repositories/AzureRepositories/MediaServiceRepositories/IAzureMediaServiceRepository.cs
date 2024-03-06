using Azure.ResourceManager.Media;
using Azure.ResourceManager.Media.Models;
using OnlineCoursePlatform.Models.AzureMediaServices;

namespace OnlineCoursePlatform.Repositories.AzureRepositories.MediaServiceRepositories
{
    public interface IAzureMediaServiceRepository
    {
        MediaServicesAccountResource? GetAzureMediaServiceAccountResource();

        Task<MediaTransformResource> CreateMediaTransformResourceAsync(
            MediaServicesAccountResource mediaServicesAccount, string transformName);

        Task<MediaTransformResource?> GetMediaTransformResourceAsync(
            MediaServicesAccountResource mediaServicesAccountResource,
            string transformName);
        Task<MediaJobResource> SubmitJobAsync(
            MediaTransformResource transform,
            string jobName,
            MediaAssetResource inputAsset,
            MediaAssetResource outputAsset, 
            string connectionId);

        Task<MediaJobResource> WaitForJobToFinishAsync(
            MediaJobResource job, string connectionId);

        Task<MediaJobResource?> GetMediaJobResourceAsync(
            MediaTransformResource mediaTransformResource,
            string jobName);

        Task<MediaAssetResource> CreateMediaInputAssetResourceAsync(
            MediaServicesAccountResource mediaServicesAccount,
            string assetName,
            IFormFile fileToUpload,
            string connectionId);

        Task<MediaAssetResource> CreateMediaOutputAssetResourceAsync(
            MediaServicesAccountResource mediaServicesAccount,
            string assetName);

        Task<MediaAssetResource?> GetMediaAssetResourceAsync(
            MediaServicesAccountResource mediaServicesAccountResource,
            string assetName);

        Task DeleteMediaAssetResourceAsync(MediaAssetResource mediaAssetResource);

        Task<StreamingLocatorResource> CreateStreamingLocatorResourceAsync(
            MediaServicesAccountResource mediaServicesAccount,
            MediaAssetResource asset,
            string locatorName,
            string contentPolicyName,
            string streamingPolicyName,
            string connectionId);

        Task<StreamingLocatorResource?> GetStreamingLocatorResourceAsync(
            MediaServicesAccountResource mediaServicesAccountResource,
            string locatorName);

        Task DeleteStreamingLocatorResource(StreamingLocatorResource streamingLocatorResource);

        Task<StreamingEndpointResource> GetStreamingEndpointResourceAsync(
            MediaServicesAccountResource mediaServicesAccount);
        
        Task StopStreamingEndpointResourceAsync(StreamingEndpointResource streamingEndpoint);
        
        Task StartStreamingEndpointResourceAsync(StreamingEndpointResource streamingEndpoint);

        ContentKeyPolicySymmetricTokenKey GetContentKeyPolicySymmetricTokenKey();

        Task<ContentKeyPolicyResource> CreateContentKeyPolicyPlayReadyResourceAsync(
            MediaServicesAccountResource mediaServicesAccountResource,
            string contentKeyPolicyName,
            ContentKeyPolicySymmetricTokenKey ckTokenSigningKey);
        
        Task<ContentKeyPolicyResource> CreateContentKeyPolicyResourceOfflinePlayReadyAndWidevineAsync(
            MediaServicesAccountResource mediaServicesAccountResource,
            string contentKeyPolicyName,
            ContentKeyPolicySymmetricTokenKey ckTokenSigningKey,
            string connectionId);

        Task<ContentKeyPolicyResource> GetContentKeyPolicyResourceAsync(
            MediaServicesAccountResource mediaServicesAccountResource,
            string contentKeyPolicyName);

        Task DeleteContentKeyPolicyResourceAsync(ContentKeyPolicyResource contentKeyPolicyResource);

        Task<PathStreamingModel> GetUrlStreamingAsync(
            StreamingLocatorResource locator,
            StreamingEndpointResource streamingEndpoint);

        string GetToken( string keyIdentifier,
            ContentKeyPolicySymmetricTokenKey ckTokenKey);

        Task CleanUpAsync(MediaTransformResource? transform,
            MediaJobResource? job,
            MediaAssetResource? inputAsset,
            MediaAssetResource? outputAsset,
            StreamingLocatorResource? streamingLocator,
            bool stopEndpoint,
            StreamingEndpointResource? streamingEndpoint,
            ContentKeyPolicyResource? contentKeyPolicy);
    }
}