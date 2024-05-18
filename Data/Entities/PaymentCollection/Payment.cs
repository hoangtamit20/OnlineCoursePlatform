using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OnlineCoursePlatform.Data.Entities.Order;

namespace OnlineCoursePlatform.Data.Entities.PaymentCollection
{
    [Table("Payment")]
    public class Payment
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [StringLength(255)]
        public string? PaymentContent { get; set; }

        [StringLength(10)]
        public string? PaymentCurrency { get; set; }

        [Column(TypeName = "decimal(19, 2)")]
        public decimal? RequiredAmount { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime")]
        public DateTime? ExpireDate { get; set; }

        [StringLength(10)]
        public string? PaymentLanguage { get; set; }

        [Column(TypeName = "decimal(19, 2)")]
        public decimal? PaidAmount { get; set; }

        [StringLength(20)]
        public string? PaymentStatus { get; set; }

        [StringLength(255)]
        public string? PaymentLastMessage { get; set; }

        public DateTime? LastUpdateAt { get; set; }

        public string? LastUpdateBy { get; set; }

        public string? MerchantId { get; set; }

        public string PaymentDestinationId { get; set; } = null!;

        public string OrderCourseId { get; set; } = null!;

        [ForeignKey("MerchantId")]
        [InverseProperty("Payments")]
        public virtual Merchant Merchant { get; set; } = null!;

        [ForeignKey("PaymentDestinationId")]
        [InverseProperty("Payments")]
        public virtual PaymentDestination PaymentDestination { get; set; } = null!;

        [ForeignKey("OrderCourseId")]
        [InverseProperty("Payments")]
        public virtual OrderCourse OrderCourse { get; set; } = null!;

        [InverseProperty("Payment")]
        public virtual ICollection<PaymentNotification> PaymentNotifications { get; set; } = new List<PaymentNotification>();

        [InverseProperty("Payment")]
        public virtual ICollection<PaymentSignature> PaymentSignatures { get; set; } = new List<PaymentSignature>();

        [InverseProperty("Payment")]
        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    }
}