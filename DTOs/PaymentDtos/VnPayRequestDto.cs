using System.Globalization;
using System.Net;
using System.Text;
using OnlineCoursePlatform.Helpers.VnPayHelpers;

namespace OnlineCoursePlatform.DTOs.PaymentDtos
{
     public class VnPayRequestDto
    {
        /// <summary>
        /// 
        /// </summary>
        public SortedList<string, string> requestData 
        = new SortedList<string, string>(new VnPayCompare());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="version"></param>
        /// <param name="tmnCode"></param>
        /// <param name="createDate"></param>
        /// <param name="ipAddress"></param>
        /// <param name="amount"></param>
        /// <param name="currCode"></param>
        /// <param name="orderType"></param>
        /// <param name="orderInfo"></param>
        /// <param name="returnUrl"></param>
        /// <param name="txnRef"></param>
        public VnPayRequestDto(string version, string tmnCode, DateTime createDate, string ipAddress,
            decimal amount, string currCode, string orderType, string orderInfo,
            string returnUrl, string txnRef)
        {
            this.vnp_Locale = "vn";
            this.vnp_IpAddr = ipAddress;
            this.vnp_Version = version;
            this.vnp_CurrCode = currCode;
            this.vnp_CreateDate = createDate.ToString("yyyyMMddHHmmss");
            this.vnp_TmnCode = tmnCode;
            this.vnp_Amount = (int)amount * 100;
            this.vnp_Command = "pay";
            this.vnp_OrderType = orderType;
            this.vnp_OrderInfo = orderInfo;
            this.vnp_ReturnUrl = returnUrl;
            this.vnp_TxnRef = txnRef;
        }

        /// <summary>
        /// 
        /// </summary>
        public VnPayRequestDto() { }

        /// <summary>
        /// Create PaymentUrl
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        public string GetLink(string baseUrl, string secretKey)
        {
            MakeRequestData();
            StringBuilder data = new StringBuilder();
            foreach(KeyValuePair<string, string> kv in requestData)
            {
                if(!String.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }

            string result = baseUrl + "?" + data.ToString();
            var secureHash = HasHelper.HmacSHA512(secretKey, data.ToString().Remove(data.Length - 1, 1));
            return result += "vnp_SecureHash=" + secureHash;
        }


        /// <summary>
        /// 
        /// </summary>
        public void MakeRequestData()
        {
            if (vnp_Amount != null)
                requestData.Add("vnp_Amount", vnp_Amount.ToString() ?? string.Empty);
            if (vnp_Command != null)
                requestData.Add("vnp_Command", vnp_Command);
            if (vnp_CreateDate != null)
                requestData.Add("vnp_CreateDate", vnp_CreateDate);
            if (vnp_CurrCode != null)
                requestData.Add("vnp_CurrCode", vnp_CurrCode);
            if (vnp_BankCode != null)
                requestData.Add("vnp_BankCode", vnp_BankCode);
            if (vnp_IpAddr != null)
                requestData.Add("vnp_IpAddr", vnp_IpAddr);
            if (vnp_Locale != null)
                requestData.Add("vnp_Locale", vnp_Locale);
            if (vnp_OrderInfo != null)
                requestData.Add("vnp_OrderInfo", vnp_OrderInfo);
            if (vnp_OrderType != null)
                requestData.Add("vnp_OrderType", vnp_OrderType);
            if (vnp_ReturnUrl != null)
                requestData.Add("vnp_ReturnUrl", vnp_ReturnUrl);
            if (vnp_TmnCode != null)
                requestData.Add("vnp_TmnCode", vnp_TmnCode);
            if (vnp_ExpireDate != null)
                requestData.Add("vnp_ExpireDate", vnp_ExpireDate);
            if (vnp_TxnRef != null)
                requestData.Add("vnp_TxnRef", vnp_TxnRef);
            if (vnp_Version != null)
                requestData.Add("vnp_Version", vnp_Version);
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal? vnp_Amount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? vnp_Command { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? vnp_CreateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? vnp_CurrCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? vnp_BankCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? vnp_IpAddr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? vnp_Locale { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? vnp_OrderInfo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? vnp_OrderType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? vnp_ReturnUrl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? vnp_TmnCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? vnp_ExpireDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? vnp_TxnRef { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? vnp_Version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? vnp_SecureHash { get; set; }
    }


    public class VnPayCompare : IComparer<string>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}