using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OnlineCoursePlatform.Data.Entities.PaymentCollection
{
    /// <summary>
    /// 
    /// </summary>
    [Table("PaymentTransaction")]
    public partial class PaymentTransaction
    {
        [Key]
        public string Id { get; private set; } = Guid.NewGuid().ToString();

        [StringLength(255)]
        public string? TranMessage { get; set; }

        public string? TranPayload { get; set; }

        [StringLength(10)]
        public string? TranStatus { get; set; }

        [Column(TypeName = "decimal(19, 2)")]
        public decimal? TranAmount { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? TranDate { get; set; }

        public string PaymentId { get; set; } = null!;

        [ForeignKey("PaymentId")]
        [InverseProperty("PaymentTransactions")]
        public virtual Payment Payment { get; set; } = null!;
    }
}