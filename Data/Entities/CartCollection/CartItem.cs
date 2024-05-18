using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OnlineCoursePlatform.Data.Entities.CartCollection
{
    [PrimaryKey("CartId", "CourseId")]
    public class CartItem
    {
        [Key]
        public string CartId { get; set; } = null!;
        [Key]
        public int CourseId { get; set; }
        public DateTime DateAdd { get; set; } = DateTime.UtcNow;
        [ForeignKey("CartId")]
        [InverseProperty("CartItems")]
        public virtual Cart Cart { get; set; } = null!;
        [ForeignKey("CourseId")]
        [InverseProperty("CartItems")]
        public virtual Course Course { get; set; } = null!;
    }
}