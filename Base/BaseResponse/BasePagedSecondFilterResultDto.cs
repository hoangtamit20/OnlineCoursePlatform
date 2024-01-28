namespace OnlineCoursePlatform.Base.BaseResponse
{
    public class BasePagedSecondFilterResultDto<T> : BasePagedResultDto<T>
    {
        public string? SecondFilter { get; set; }
    }
}