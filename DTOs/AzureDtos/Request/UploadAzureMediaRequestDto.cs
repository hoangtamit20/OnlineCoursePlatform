using OnlineCoursePlatform.Data.Entities;

namespace OnlineCoursePlatform.DTOs.AzureDtos.Request
{
    public class UploadAzureMediaRequestDto<T>
    {
        public string InputAssetName { get; set; } = null!;
        public string OutputAssetName { get; set; } = null!;
        public string LocatorName { get; set; } = null!;
        public string JobName { get; set; } = null!;
        public string TransformName { get; set; } = null!;
        public IFormFile FileToUpload { get; set; } = null!;
        public string ContentKeyPolicyName { get; set; } = null!;

        public UploadAzureMediaRequestDto(AppUser user, T entity)
        {
            var unique = Guid.NewGuid().ToString()[..13];
            if (entity is Lesson)
            {
                Lesson? lesson = entity as Lesson;
                InputAssetName = $"input-email-{user.Email?.Split('@')[0]}-lesson-{lesson?.Id}-{unique}";
                OutputAssetName = $"output-email-{user.Email?.Split('@')[0]}-lesson-{lesson?.Id}-{unique}";
                LocatorName = $"locator-email-{user.Email?.Split('@')[0]}-lesson-{lesson?.Id}-{unique}";
                JobName = $"job-email-{user.Email?.Split('@')[0]}-lesson-{lesson?.Id}-{unique}";
                ContentKeyPolicyName = $"contentkeypolicy-email-{user.Email?.Split('@')[0]}-lesson-{lesson?.Id}-{unique}";
                // TransformName = $"transform-email-{user.Email?.Split('@')[0]}-lesson-{lesson?.Id}-{unique}";
                TransformName = $"MyTransformWithAdaptiveStreamingPreset";
            }
            else if (entity is Course)
            {
                Course? course = entity as Course;
                InputAssetName = $"input-email-{user.Email?.Split('@')[0]}-course-{course?.Id}-{unique}";
                OutputAssetName = $"output-email-{user.Email?.Split('@')[0]}-course-{course?.Id}-{unique}";
                LocatorName = $"locator-email-{user.Email?.Split('@')[0]}-course-{course?.Id}-{unique}";
                JobName = $"job-email-{user.Email?.Split('@')[0]}-course-{course?.Id}-{unique}";
                ContentKeyPolicyName = $"contentkeypolicy-email-{user.Email?.Split('@')[0]}-course-{course?.Id}-{unique}";
                // TransformName = $"transform-email-{user.Email?.Split('@')[0]}-course-{course?.Id}-{unique}";
                TransformName = $"MyTransformWithAdaptiveStreamingPreset";
            }
        }
    }
}