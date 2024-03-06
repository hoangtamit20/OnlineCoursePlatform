using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Media;
using Azure.ResourceManager.Media.Models;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using OnlineCoursePlatform.Configurations;
using OnlineCoursePlatform.Constants;
using OnlineCoursePlatform.Hubs;
using OnlineCoursePlatform.Models.AzureMediaServices;

namespace OnlineCoursePlatform.Repositories.AzureRepositories.MediaServiceRepositories
{
    public class AzureMediaServiceRepository : IAzureMediaServiceRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureMediaServiceRepository> _logger;
        private readonly IHubContext<ProgressHub> _hubContext;

        public AzureMediaServiceRepository(
            IConfiguration configuration,
            ILogger<AzureMediaServiceRepository> logger,
            IHubContext<ProgressHub> hubContext)
        {
            _configuration = configuration;
            _logger = logger;
            _hubContext = hubContext;
        }

        public MediaServicesAccountResource? GetAzureMediaServiceAccountResource()
        {
            var mediaServiceAccountId = MediaServicesAccountResource.CreateResourceIdentifier(
                subscriptionId: _configuration[AppSettingsConfig.AZURE_SUBSCRIPTION_ID],
                resourceGroupName: _configuration[AppSettingsConfig.AZURE_RESOURCE_GROUP],
                accountName: _configuration[AppSettingsConfig.AZURE_MEDIA_SERVICES_ACCOUNT_NAME]);

            // var credential = new DefaultAzureCredential(includeInteractiveCredentials: true);
            TokenCredential credential = new ClientSecretCredential(
                tenantId: _configuration[AppSettingsConfig.AZURE_TENANT_ID],
                clientId: _configuration[AppSettingsConfig.AZURE_APP_CLIENT_ID],
                clientSecret: _configuration[AppSettingsConfig.AZURE_APP_CLIENT_SECRET]);
            var armClient = new ArmClient(credential);

            return armClient.GetMediaServicesAccountResource(id: mediaServiceAccountId);
        }

        public async Task<MediaTransformResource> CreateMediaTransformResourceAsync(
            MediaServicesAccountResource mediaServicesAccount,
            string transformName)
        {
            var transform = await mediaServicesAccount.GetMediaTransforms()
                .CreateOrUpdateAsync(waitUntil: WaitUntil.Completed,
                transformName: transformName,
                data: new MediaTransformData
                {
                    Outputs = {
                        new MediaTransformOutput(
                            // The preset for the Transform is set to one of Media Services built-in sample presets.
                            // You can  customize the encoding settings by changing this to use "StandardEncoderPreset" class.
                            preset: new BuiltInStandardEncoderPreset(
                                // This sample uses the built-in encoding preset for Adaptive Bit-rate Streaming.
                                presetName: EncoderNamedPreset.AdaptiveStreaming
                            )
                        )
                    }
                });
            return transform.Value;
        }

        public async Task<MediaTransformResource?> GetMediaTransformResourceAsync(
            MediaServicesAccountResource mediaServicesAccountResource,
            string transformName)
        => await mediaServicesAccountResource.GetMediaTransformAsync(transformName: transformName);

        public async Task<MediaJobResource> SubmitJobAsync(
            MediaTransformResource transform,
            string jobName,
            MediaAssetResource inputAsset,
            MediaAssetResource outputAsset,
            string connectionId)
        {
            // In this example, we are assuming that the Job name is unique.
            //
            // If you already have a Job with the desired name, use the Jobs.Get method
            // to get the existing Job. In Media Services v3, Get methods on entities returns ErrorResponseException 
            // if the entity doesn't exist (a case-insensitive check on the name).
            _logger.LogInformation("Creating a Job...");
            await _hubContext.Clients.Client(connectionId: connectionId).SendAsync(method: HubConstants.ReceiveProgress, arg1: "Creating a Job...");

            var job = await transform.GetMediaJobs().CreateOrUpdateAsync(
                waitUntil: WaitUntil.Completed,
                jobName: jobName,
                data: new MediaJobData
                {
                    Input = new MediaJobInputAsset(assetName: inputAsset.Data.Name),
                    Outputs =
                    {
                        new MediaJobOutputAsset(outputAsset.Data.Name)
                    }
                });

            return job.Value;
        }

        // public async Task<MediaJobResource> WaitForJobToFinishAsync(
        //     MediaJobResource job)
        // {
        //     var sleepInterval = TimeSpan.FromSeconds(30);
        //     MediaJobState state;

        //     do
        //     {
        //         job = await job.GetAsync();
        //         state = job.Data.State.GetValueOrDefault();
        //         _logger.LogInformation($"Job is '{state}'.");
        //         for (int i = 0; i < job.Data.Outputs.Count; i++)
        //         {
        //             var output = job.Data.Outputs[i];
        //             _logger.LogInformation($"\tJobOutput[{i}] is '{output.State}'.");
        //             if (output.State == MediaJobState.Processing)
        //             {
        //                 _logger.LogInformation($"  Progress: '{output.Progress}'.");
        //             }
        //         }
        //         if (state != MediaJobState.Finished
        //             && state != MediaJobState.Error
        //             && state != MediaJobState.Canceled)
        //         {
        //             await Task.Delay(sleepInterval);
        //         }
        //     }
        //     while (state != MediaJobState.Finished
        //         && state != MediaJobState.Error
        //         && state != MediaJobState.Canceled);
        //     return job;
        // }


        public async Task<MediaJobResource> WaitForJobToFinishAsync(MediaJobResource job, string connectionId)
        {
            var sleepInterval = TimeSpan.FromSeconds(30);
            MediaJobState state;

            do
            {
                job = await job.GetAsync();
                state = job.Data.State.GetValueOrDefault();
                _logger.LogInformation($"Job is '{state}'.");
                await _hubContext.Clients.Client(connectionId: connectionId).SendAsync(method: HubConstants.ReceiveProgress, arg1: $"Job is '{state}'.");

                for (int i = 0; i < job.Data.Outputs.Count; i++)
                {
                    var output = job.Data.Outputs[i];
                    _logger.LogInformation($"\tJobOutput[{i}] is '{output.State}'.");
                    await _hubContext.Clients
                        .Client(connectionId: connectionId)
                        .SendAsync(method: HubConstants.ReceiveProgress, arg1: $"\tJobOutput[{i}] is '{output.State}'.");
                    if (output.State == MediaJobState.Processing)
                    {
                        var progressPercentage = output.Progress * 100; // Assuming output.Progress is a value between 0 and 1
                        var progressMessage = $"Job '{job.Data.Name}' is processing {progressPercentage}%";
                        _logger.LogInformation(progressMessage);
                        await _hubContext.Clients.Client(connectionId: connectionId).SendAsync(method: HubConstants.ReceiveProgress, arg1: progressMessage);
                    }

                }
                if (state != MediaJobState.Finished
                    && state != MediaJobState.Error
                    && state != MediaJobState.Canceled)
                {
                    await Task.Delay(sleepInterval);
                }
            }
            while (state != MediaJobState.Finished
                && state != MediaJobState.Error
                && state != MediaJobState.Canceled);
            return job;
        }


        public async Task<MediaJobResource?> GetMediaJobResourceAsync(
            MediaTransformResource mediaTransformResource,
            string jobName)
        => await mediaTransformResource.GetMediaJobAsync(jobName: jobName);

        public async Task<MediaAssetResource> CreateMediaInputAssetResourceAsync(
            MediaServicesAccountResource mediaServicesAccount,
            string assetName,
            IFormFile fileToUpload,
            string connectionId)
        {
            // In this example, we are assuming that the Asset name is unique.
            MediaAssetResource mediaAssetResource;
            try
            {
                mediaAssetResource = await mediaServicesAccount.GetMediaAssetAsync(assetName: assetName);
                // The Asset already exists and we are going to overwrite it. In your application, if you don't want to overwrite
                // an existing Asset, use an unique name.
                _logger.LogWarning($"Warning: The Asset named {assetName} already exists. It will be overwritten.");
            }
            catch (RequestFailedException)
            {
                // Call Media Services API to create an Asset.
                // This method creates a container in storage for the Asset.
                // The files (blobs) associated with the Asset will be stored in this container.
                _logger.LogInformation("Creating an input Asset...");
                await _hubContext.Clients.Client(connectionId: connectionId).SendAsync(method: HubConstants.ReceiveProgress, arg1: "Creating an input Asset...");

                mediaAssetResource = (await mediaServicesAccount
                    .GetMediaAssets()
                    .CreateOrUpdateAsync(
                        waitUntil: WaitUntil.Completed,
                        assetName: assetName,
                        data: new MediaAssetData()
                    )).Value;
            }
            // Use Media Services API to get back a response that contains
            // SAS URL for the Asset container into which to upload blobs.
            // That is where you would specify read-write permissions
            // and the expiration time for the SAS URL.
            var sasUriCollection = mediaAssetResource.GetStorageContainerUrisAsync(
                new MediaAssetStorageContainerSasContent
                {
                    Permissions = MediaAssetContainerPermission.ReadWrite,
                    ExpireOn = DateTime.UtcNow.AddHours(1)
                });

            var sasUri = await sasUriCollection.FirstOrDefaultAsync();
            // Use Storage API to get a reference to the Asset container
            // that was created by calling Asset's CreateOrUpdate method.
            var container = new BlobContainerClient(sasUri);
            string newFileName = fileToUpload.FileName.Replace('_', '-');
            BlobClient blob = container.GetBlobClient(newFileName);

            // Use Storage API to upload the file into the container in storage.
            /*In this case you must provide processing percent upload file to client as Azure SignalR Service*/
            _logger.LogInformation("Uploading a media file to the Asset...");
            using (var stream = fileToUpload.OpenReadStream())
            {
                var progressHandler = new Progress<long>();
                progressHandler.ProgressChanged += async (s, bytesUploaded) =>
                {
                    double progressPercentage = (double)bytesUploaded / fileToUpload.Length * 100;
                    string progressMessage = $"Uploading file progressing... : {progressPercentage}%";
                    _logger.LogInformation($"{progressMessage}");
                    await _hubContext.Clients.Client(connectionId: connectionId).SendAsync(method: HubConstants.ReceiveProgress, arg1: progressMessage);
                };
                await blob.UploadAsync(stream, progressHandler: progressHandler);
            }

            return mediaAssetResource;
        }

        public async Task<MediaAssetResource> CreateMediaOutputAssetResourceAsync(
            MediaServicesAccountResource mediaServicesAccount,
            string assetName)
        {
            _logger.LogInformation("Creating an output Asset...");
            var asset = await mediaServicesAccount.GetMediaAssets().CreateOrUpdateAsync(
                waitUntil: WaitUntil.Completed,
                assetName: assetName,
                data: new MediaAssetData());

            return asset.Value;
        }

        public async Task<MediaAssetResource?> GetMediaAssetResourceAsync(
            MediaServicesAccountResource mediaServicesAccountResource,
            string assetName)
        => await mediaServicesAccountResource.GetMediaAssetAsync(assetName: assetName);

        public async Task DeleteMediaAssetResourceAsync(MediaAssetResource mediaAssetResource)
        => await mediaAssetResource.DeleteAsync(waitUntil: WaitUntil.Completed);

        public async Task<StreamingLocatorResource> CreateStreamingLocatorResourceAsync(
            MediaServicesAccountResource mediaServicesAccount,
            MediaAssetResource asset,
            string locatorName,
            string contentPolicyName,
            string streamingPolicyName,
            string connectionId)
        {
            _logger.LogInformation("Creating a streaming locator...");
            await _hubContext.Clients
                .Client(connectionId: connectionId)
                .SendAsync(method: HubConstants.ReceiveProgress, arg1: "Creating a streaming locator...");

            var locator = await mediaServicesAccount.GetStreamingLocators().CreateOrUpdateAsync(
                waitUntil: WaitUntil.Completed,
                streamingLocatorName: locatorName,
                data: new StreamingLocatorData
                {
                    AssetName = asset.Data.Name,
                    StreamingPolicyName = streamingPolicyName,
                    DefaultContentKeyPolicyName = contentPolicyName
                });
            return locator.Value;
        }

        public async Task<StreamingLocatorResource?> GetStreamingLocatorResourceAsync(
            MediaServicesAccountResource mediaServicesAccountResource,
            string locatorName)
        => await mediaServicesAccountResource.GetStreamingLocatorAsync(streamingLocatorName: locatorName);

        public async Task DeleteStreamingLocatorResource(StreamingLocatorResource streamingLocatorResource)
        => await streamingLocatorResource.DeleteAsync(waitUntil: WaitUntil.Completed);

        public async Task<ContentKeyPolicyResource> GetContentKeyPolicyResourceAsync(
            MediaServicesAccountResource mediaServicesAccountResource,
            string contentKeyPolicyName)
        => await mediaServicesAccountResource.GetContentKeyPolicyAsync(
            contentKeyPolicyName: contentKeyPolicyName);

        public async Task DeleteContentKeyPolicyResourceAsync(ContentKeyPolicyResource contentKeyPolicyResource)
        => await contentKeyPolicyResource.DeleteAsync(WaitUntil.Completed);


        public ContentKeyPolicyPlayReadyConfiguration ConfigurePlayReadyPersistentLicenseTemplate()
        {
            ContentKeyPolicyPlayReadyLicense objContentKeyPolicyPlayReadyLicense;

            objContentKeyPolicyPlayReadyLicense = new ContentKeyPolicyPlayReadyLicense(
                allowTestDevices: true,
                licenseType: ContentKeyPolicyPlayReadyLicenseType.Persistent,
                contentKeyLocation: new ContentKeyPolicyPlayReadyContentEncryptionKeyFromHeader(),
                contentType: ContentKeyPolicyPlayReadyContentType.UltraVioletStreaming)
            {
                BeginOn = new DateTime(2023, 1, 15),
                PlayRight = new ContentKeyPolicyPlayReadyPlayRight(
                    hasDigitalVideoOnlyContentRestriction: false,
                    hasImageConstraintForAnalogComponentVideoRestriction: true,
                    hasImageConstraintForAnalogComputerMonitorRestriction: true,
                    allowPassingVideoContentToUnknownOutput: ContentKeyPolicyPlayReadyUnknownOutputPassingOption.Allowed)
                {
                    ExplicitAnalogTelevisionOutputRestriction = new ContentKeyPolicyPlayReadyExplicitAnalogTelevisionRestriction(true, 2),
                    AllowPassingVideoContentToUnknownOutput = ContentKeyPolicyPlayReadyUnknownOutputPassingOption.Allowed
                }
            };

            var objContentKeyPolicyPlayReadyConfiguration = new ContentKeyPolicyPlayReadyConfiguration(
                new List<ContentKeyPolicyPlayReadyLicense> { objContentKeyPolicyPlayReadyLicense });

            return objContentKeyPolicyPlayReadyConfiguration;
        }


        private ContentKeyPolicyPlayReadyConfiguration ConfigurePlayReadyNonPersistentLicenseTemplate()
        {
            ContentKeyPolicyPlayReadyLicense objContentKeyPolicyPlayReadyLicense;

            objContentKeyPolicyPlayReadyLicense = new ContentKeyPolicyPlayReadyLicense(
                allowTestDevices: true,
                licenseType: ContentKeyPolicyPlayReadyLicenseType.NonPersistent,
                contentKeyLocation: new ContentKeyPolicyPlayReadyContentEncryptionKeyFromHeader(),
                contentType: ContentKeyPolicyPlayReadyContentType.UltraVioletStreaming)
            {
                PlayRight = new ContentKeyPolicyPlayReadyPlayRight(
                    hasDigitalVideoOnlyContentRestriction: false,
                    hasImageConstraintForAnalogComponentVideoRestriction: true,
                    hasImageConstraintForAnalogComputerMonitorRestriction: true,
                    allowPassingVideoContentToUnknownOutput: ContentKeyPolicyPlayReadyUnknownOutputPassingOption.Allowed)
                {
                    ExplicitAnalogTelevisionOutputRestriction = new ContentKeyPolicyPlayReadyExplicitAnalogTelevisionRestriction(
                        isBestEffort: true, configurationData: 2),
                    AllowPassingVideoContentToUnknownOutput = ContentKeyPolicyPlayReadyUnknownOutputPassingOption.Allowed
                }
            };
            var objContentKeyPolicyPlayReadyConfiguration = new ContentKeyPolicyPlayReadyConfiguration(
                licenses: new List<ContentKeyPolicyPlayReadyLicense> { objContentKeyPolicyPlayReadyLicense });

            return objContentKeyPolicyPlayReadyConfiguration;
        }


        public ContentKeyPolicyWidevineConfiguration ConfigureWidevineLicenseTemplate()
        {
            var template = new WidevineTemplate()
            {
                AllowedTrackTypes = "SD_HD",
                ContentKeySpecs = new ContentKeySpec[]
                {
                    new ContentKeySpec()
                    {
                        TrackType = "SD",
                        SecurityLevel = 1,
                        RequiredOutputProtection = new OutputProtection()
                        {
                            HDCP = "HDCP_NONE"
                            // NOTE: the policy should be set to "HDCP_V1" (or greater) if you need to disable screen capture. The Widevine desktop
                            // browser CDM module only blocks screen capture when HDCP is enabled and the screen capture application is using
                            // Chromes screen capture APIs. 
                        }
                    }
                },
                PolicyOverrides = new PolicyOverrides()
                {
                    CanPlay = true,
                    CanPersist = true,
                    CanRenew = false,
                    RentalDurationSeconds = 2592000,
                    PlaybackDurationSeconds = 10800,
                    LicenseDurationSeconds = 604800,
                }
            };
            return new ContentKeyPolicyWidevineConfiguration(Newtonsoft.Json.JsonConvert.SerializeObject(template));
        }

        public async Task<StreamingEndpointResource> GetStreamingEndpointResourceAsync(
            MediaServicesAccountResource mediaServicesAccount)
        => (await mediaServicesAccount
            .GetStreamingEndpoints()
            .GetAsync(streamingEndpointName: "default"))
            .Value;

        public async Task StopStreamingEndpointResourceAsync(StreamingEndpointResource streamingEndpoint)
        => await streamingEndpoint.StopAsync(waitUntil: WaitUntil.Completed);

        public async Task StartStreamingEndpointResourceAsync(StreamingEndpointResource streamingEndpoint)
        {
            if (streamingEndpoint.Data.ResourceState != StreamingEndpointResourceState.Running)
            {
                _logger.LogInformation($"Streaming Endpoint is not running, starting now...");
                await streamingEndpoint.StartAsync(WaitUntil.Completed);
            }
        }


        public async Task CleanUpAsync(MediaTransformResource? transform,
            MediaJobResource? job,
            MediaAssetResource? inputAsset,
            MediaAssetResource? outputAsset,
            StreamingLocatorResource? streamingLocator,
            bool stopEndpoint,
            StreamingEndpointResource? streamingEndpoint,
            ContentKeyPolicyResource? contentKeyPolicy)
        {
            if (job is not null)
            {
                await job.DeleteAsync(WaitUntil.Completed);
            }

            if (transform is not null)
            {
                await transform.DeleteAsync(WaitUntil.Completed);
            }

            if (inputAsset != null)
            {
                await inputAsset.DeleteAsync(WaitUntil.Completed);
            }

            if (outputAsset is not null)
            {
                await outputAsset.DeleteAsync(WaitUntil.Completed);
            }

            if (streamingLocator != null)
            {
                await streamingLocator.DeleteAsync(WaitUntil.Completed);
            }

            if (contentKeyPolicy != null)
            {
                await contentKeyPolicy.DeleteAsync(WaitUntil.Completed);
            }

            if (streamingEndpoint != null)
            {
                if (stopEndpoint)
                {
                    // Because we started the endpoint, we'll stop it.
                    await streamingEndpoint.StopAsync(WaitUntil.Completed);
                }
                else
                {
                    // We will keep the endpoint running because it was not started by us. There are costs to keep it running.
                    // Please refer https://azure.microsoft.com/en-us/pricing/details/media-services/ for pricing. 
                    _logger.LogInformation($"The Streaming Endpoint '{streamingEndpoint.Data.Name}' is running. To stop further billing for the Streaming Endpoint, please stop it using the Azure portal.");
                }
            }
        }

        public ContentKeyPolicySymmetricTokenKey GetContentKeyPolicySymmetricTokenKey()
        {
            string tokenKey = _configuration[AppSettingsConfig.AZURE_CONTENT_TOKEN_KEY]!;
            byte[] tokenSigningKey = Encoding.UTF8.GetBytes(s: tokenKey);
            return new ContentKeyPolicySymmetricTokenKey(keyValue: tokenSigningKey);
        }

        public async Task<ContentKeyPolicyResource> CreateContentKeyPolicyPlayReadyResourceAsync(
            MediaServicesAccountResource mediaServicesAccountResource,
            string contentKeyPolicyName,
            ContentKeyPolicySymmetricTokenKey ckTokenSigningKey)
        {
            _logger.LogInformation($"Creating a content key policy ...");
            var contentKeyPolicy = await mediaServicesAccountResource.GetContentKeyPolicies()
                .CreateOrUpdateAsync(
                    waitUntil: WaitUntil.Completed,
                    contentKeyPolicyName: contentKeyPolicyName,
                    data: new ContentKeyPolicyData
                    {
                        Options =
                        {
                            new ContentKeyPolicyOption(
                                configuration: ConfigurePlayReadyNonPersistentLicenseTemplate(),
                                restriction: new ContentKeyPolicyTokenRestriction(
                                    issuer: _configuration[AppSettingsConfig.JWT_ISSUER],
                                    audience: _configuration[AppSettingsConfig.JWT_AUDIENCE],
                                    primaryVerificationKey: ckTokenSigningKey,
                                    restrictionTokenType: ContentKeyPolicyRestrictionTokenType.Jwt)
                                {
                                    RequiredClaims = {
                                        new ContentKeyPolicyTokenClaim {
                                            ClaimType = "urn:microsoft:azure:mediaservices:contentkeyidentifier"
                                        }
                                    }
                                }

                            )
                        }
                    });
            return contentKeyPolicy.Value;
        }


        public async Task<ContentKeyPolicyResource> CreateContentKeyPolicyResourceOfflinePlayReadyAndWidevineAsync(
            MediaServicesAccountResource mediaServicesAccount,
            string contentKeyPolicyName,
            ContentKeyPolicySymmetricTokenKey ckTokenSigningKey,
            string connectionId)
        {
            _logger.LogInformation($"Creating a content key policy with offline playready and widevine ...");
            await _hubContext.Clients
                .Client(connectionId: connectionId)
                .SendAsync(method: HubConstants.ReceiveProgress, arg1: "Create a protection policy content ...");

            var tokenRestriction = new ContentKeyPolicyTokenRestriction(
                issuer: _configuration[AppSettingsConfig.JWT_ISSUER],
                audience: _configuration[AppSettingsConfig.JWT_AUDIENCE],
                primaryVerificationKey: ckTokenSigningKey,
                restrictionTokenType: ContentKeyPolicyRestrictionTokenType.Jwt)
            {
                RequiredClaims = {
                        new ContentKeyPolicyTokenClaim {
                            ClaimType = "urn:microsoft:azure:mediaservices:contentkeyidentifier"
                        }
                    }
            };
            var policy = await mediaServicesAccount.GetContentKeyPolicies().CreateOrUpdateAsync(
                waitUntil: WaitUntil.Completed,
                contentKeyPolicyName: contentKeyPolicyName,
                data: new ContentKeyPolicyData
                {
                    Options =
                    {
                        new ContentKeyPolicyOption(
                            configuration: ConfigurePlayReadyPersistentLicenseTemplate(),
                            restriction: tokenRestriction),
                        new ContentKeyPolicyOption(
                            configuration: ConfigureWidevineLicenseTemplate(),
                            restriction: tokenRestriction
                        )
                    }
                }
            );
            return policy.Value;
        }

        public async Task<PathStreamingModel> GetUrlStreamingAsync(
            StreamingLocatorResource locator,
            StreamingEndpointResource streamingEndpoint)
        {
            PathStreamingModel pathStreamingModel = new PathStreamingModel();
            var paths = await locator.GetStreamingPathsAsync();
            foreach (var path in paths.Value.StreamingPaths)
            {
                var protocol = path.StreamingProtocol;
                foreach (var url in path.Paths)
                {
                    var uri = new UriBuilder()
                    {
                        Scheme = "https",
                        Host = streamingEndpoint.Data.HostName,
                        Path = url
                    }.ToString();
                    if (protocol == StreamingPolicyStreamingProtocol.Hls)
                    {
                        if (uri.Contains("cmaf"))
                        {
                            pathStreamingModel.HlsCmafUrl = uri;
                        }
                        else
                        {
                            pathStreamingModel.HlsStandardUrl = uri;
                        }
                    }
                    else if (protocol == StreamingPolicyStreamingProtocol.Dash)
                    {
                        if (uri.Contains("cmaf"))
                        {
                            pathStreamingModel.DashCmafUrl = uri;
                        }
                        else
                        {
                            pathStreamingModel.DashStandardUrl = uri;
                        }
                    }
                    else if (protocol == StreamingPolicyStreamingProtocol.SmoothStreaming)
                    {
                        pathStreamingModel.SmoothStreamingUrl = uri;
                    }
                    else if (protocol == StreamingPolicyStreamingProtocol.Download)
                    {
                        _logger.LogInformation($"DOWNLOAD URL : {uri}");
                    }
                }
            }
            return pathStreamingModel;
        }


        public string GetToken(
            string keyIdentifier,
            ContentKeyPolicySymmetricTokenKey ckTokenKey)
        {
            var tokenSigningKey = new SymmetricSecurityKey(ckTokenKey.KeyValue);
            var credential = new SigningCredentials(
                key: tokenSigningKey,
                // Use the  HmacSha256 and not the HmacSha256Signature option, or the token will not work!
                algorithm: SecurityAlgorithms.HmacSha256,
                digest: SecurityAlgorithms.Sha256Digest);
            var claims = new List<Claim>()
            {
                new Claim(type: "urn:microsoft:azure:mediaservices:contentkeyidentifier", value: keyIdentifier),
                new Claim(type: "urn:microsoft:azure:mediaservices:maxuses", value: "5")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration[AppSettingsConfig.JWT_ISSUER],
                audience: _configuration[AppSettingsConfig.JWT_AUDIENCE],
                claims: claims,
                notBefore: DateTime.UtcNow.AddMinutes(-5),
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: credential);
            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(token: token);
        }
    }
}