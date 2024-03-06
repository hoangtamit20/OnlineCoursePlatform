using OnlineCoursePlatform.Data.Entities;

namespace OnlineCoursePlatform.Models.AzureMediaServices
{
    public class AddFilesMediaServiceModel
    {
        public string JobName { get; set; } = null!;
        public string LocatorName { get; set; } = null!;
        public string InputAssetName { get; set; } = null!;
        public string OutputAssetName { get; set; } = null!;
        public string ContentKeyPolicyName { get; set; } = null!;
        public bool StopStreamingEndpoint { get; set; } = false;

        public AddFilesMediaServiceModel(AppUser user)
        {
            string uniqueness = Guid.NewGuid().ToString()[..13];
            JobName = $"job-{uniqueness}";
            LocatorName = $"locator-{user.Email}-{uniqueness}";
            InputAssetName = $"input-{uniqueness}";
            OutputAssetName = $"output-{user.Email}-{uniqueness}";
            ContentKeyPolicyName = $"contentkeypolicy-{uniqueness}";
            // InputAssetName = $"input-{user.Email}-CourseId:{courseId}-{DateTime.UtcNow.ToString()}-{uniqueness}";
            // OutputAssetName = $"output-{user.Email}-CourseId:{courseId}-{DateTime.UtcNow.ToString()}-{uniqueness}";
            StopStreamingEndpoint = false;
        }
    }
}