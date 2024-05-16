using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OnlineCoursePlatform.Data.Entities.PaymentCollection;

namespace OnlineCoursePlatform.Data.Entities.Order
{
    [Table("OrderCourse")]
    public class OrderCourse
    {
        [Key]
        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")]
        [InverseProperty("OrderCourses")]
        public virtual AppUser User { get; set; } = null!;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(19, 2)")]
        public decimal TotalPrice { get; set; } = 0;

        public  OrderStatus Status { get; set; }

        [InverseProperty("OrderCourse")]
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        [InverseProperty("OrderCourse")]
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

    public enum OrderStatus
    {
        Progressing = 1,
        Draft = 2,
        Success = 3,
        Cancel = 4
    }
}