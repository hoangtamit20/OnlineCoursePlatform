using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OnlineCoursePlatform.Data.Entities.Order
{
    [PrimaryKey("OrderCourseId", "CourseId")]
    [Table("OrderDetail")]
    public partial class OrderDetail
    {
        [Key]
        public int OrderCourseId { get; set; }

        [Key]
        public int CourseId { get; set; }

        [Column(TypeName = "decimal(19, 2)")]
        public decimal Price { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime ExpireDate { get; set; }

        [ForeignKey("OrderCourseId")]
        [InverseProperty("OrderDetails")]
        public virtual OrderCourse OrderCourse { get; set; } = null!;

        [ForeignKey("CourseId")]
        [InverseProperty("OrderDetails")]
        public virtual Course Course { get; set; } = null!;
    }
}