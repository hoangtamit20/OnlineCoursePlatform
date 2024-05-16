using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Data.Entities.PaymentCollection
{
    [Table("PaymentSignature")]
    public class PaymentSignature
    {
        [Key]
        public string Id { get; private set; } = Guid.NewGuid().ToString();

        [StringLength(500)]
        public string? SignValue { get; set; }

        [StringLength(50)]
        public string? SignAlgo { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? SignDate { get; set; }

        [StringLength(60)]
        public string? SignOwn { get; set; }

        public bool IsValid { get; set; }

        public string PaymentId { get; set; } = null!;

        [ForeignKey("PaymentId")]
        [InverseProperty("PaymentSignatures")]
        public virtual Payment Payment { get; set; } = null!;
    }

}