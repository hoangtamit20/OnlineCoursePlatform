using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Data.Entities.PaymentCollection
{
    [Table("PaymentDestination")]
    public partial class PaymentDestination : BaseAuditableEntity
    {
        [Key]
        public string Id { get; private set; } = Guid.NewGuid().ToString();

        [StringLength(255)]
        public string? DesLogo { get; set; }

        [StringLength(50)]
        public string? DesShortName { get; set; }

        [StringLength(255)]
        public string? DesName { get; set; }

        public int? DesSortIndex { get; set; }

        public string? ParentId { get; set; }

        public bool? IsActive { get; set; }

        [InverseProperty("Parent")]
        public virtual ICollection<PaymentDestination> InverseParent { get; set; } = new List<PaymentDestination>();

        [ForeignKey("ParentId")]
        [InverseProperty("InverseParent")]
        public virtual PaymentDestination? Parent { get; set; }

        [InverseProperty("PaymentDestination")]
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }



    public class BaseAuditableEntity
    {
        [StringLength(50)]
        public string? CreateBy { get; set; }


        [Column(TypeName = "datetime")]
        public DateTime? CreateAt { get; set; }
        
        [StringLength(50)]
        public string? LastUpdateBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? LastUpdateAt { get; set; }
    }
}