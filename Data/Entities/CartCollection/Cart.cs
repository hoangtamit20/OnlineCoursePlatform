using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OnlineCoursePlatform.Data.Entities.CartCollection;

namespace OnlineCoursePlatform.Data.Entities
{
    public class Cart
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = null!;

        [Required]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [InverseProperty("Cart")]
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}