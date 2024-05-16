using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OnlineCoursePlatform.Data.Entities.PaymentCollection;

namespace OnlineCoursePlatform.Data.Entities.Order
{

    [Table("Merchant")]
    public partial class Merchant
    {
        [Key]
        public string Id { get; private set; } = Guid.NewGuid().ToString();

        [StringLength(50)]
        public string? MerchantName { get; set; }

        [StringLength(255)]
        public string? MerchantWebLink { get; set; }

        [StringLength(255)]
        public string? MerchantIpnUrl { get; set; }

        [StringLength(255)]
        public string? MerchantReturnUrl { get; set; }

        [StringLength(50)]
        public string? SecretKey { get; set; }

        public bool? IsActive { get; set; }

        [InverseProperty("Merchant")]
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}