using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Data.Entities.CourseDiscount
{
    public class Promotion
    {
        // public string Id { get; private set; } = null!;
        // public string Name { get; set; } = null!;
        // public DiscountType DiscountType { get; set; }
        // public decimal DiscountValue { get; set; }
        // public DateTime DateCreate { get; set; } = DateTime.UtcNow;
        // public DateTime EffectiveDate { get; set; }
        // public DateTime EffectiveTo { get; set; }
        // [InverseProperty("Promotion")]
        // public ICollection<CoursePromotion> CoursePromotions { get; set; } = new List<CoursePromotion>();
    }

    public enum DiscountType
    {
        Percent = 1,
        Value = 2
    }
}