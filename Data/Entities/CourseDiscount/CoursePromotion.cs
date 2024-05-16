using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Data.Entities.CourseDiscount
{
    public class CoursePromotion
    {
        // [Key]
        // public string Id { get; set; } = Guid.NewGuid().ToString();
        // public int CourseId { get; set; }
        // public string PromotionId { get; set; } = null!;
        // [ForeignKey("CourseId")]
        // [InverseProperty("CoursePromotions")]
        // public virtual Course Course { get; set; } = null!;
        // [ForeignKey("PromotionId")]
        // [InverseProperty("CoursePromotions")]
        // public virtual Promotion Promotion { get; set; } = null!;
    }
}