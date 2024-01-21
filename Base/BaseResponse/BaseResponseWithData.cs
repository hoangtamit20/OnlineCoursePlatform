namespace OnlineCoursePlatform.Base.BaseResponse
{
    public class BaseResponseWithData <T> : BaseResponseDto
    {
        public T? Data { get; set; }
        public List<string>? Errors { get; set; } = new();
    }
}