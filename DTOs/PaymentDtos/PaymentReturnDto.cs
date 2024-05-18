using System.Net;

namespace OnlineCoursePlatform.DTOs.PaymentDtos
{
    public class PaymentReturnDto
    {
        /// <summary>
        /// 
        /// </summary>
        public string PaymentId { get; set; } = null!;

        /// <summary>
        /// 00: Success
        /// 99: Unknown
        /// 10: Error
        /// </summary>
        public string? PaymentStatus { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? PaymentMessage { get; set; }

        /// <summary>
        /// Format: yyyyMMddHHmmss
        /// </summary>
        public string? PaymentDate { get; set; }

        /// <summary>
        /// Mã để Merchant xử lý kết quả
        /// </summary>
        public string? PaymentRefId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Chữ ký để Merchant xác nhận
        /// </summary>
        public string? Signature { get; set; }
    }


    public static class ObjectHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToQueryString(this object obj)
        {
            var properties = obj.GetType().GetProperties()
                .Where(o => o.GetValue(obj, null) != null)
                .Select(o => $"{o.Name}={WebUtility.UrlEncode(o.GetValue(obj, null)?.ToString())}");
            return string.Join("&", properties.ToArray());
        }
    }
}