namespace OnlineCoursePlatform.DTOs.PaymentDtos
{
    public class VnPayIpnResponseDto : VnPayResponseDto
    {
        /// <summary>
        /// 
        /// </summary>
        public string RspCode { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rspCode"></param>
        /// <param name="message"></param>
        public void Set(string rspCode, string message)
        {
            RspCode = rspCode;
            Message = message;
        }
    }
}