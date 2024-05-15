using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineCoursePlatform.Data.Entities.Chat
{
    public class OrderCourse
    {
        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course Course { get; set; } = null!;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime ExpireDate { get; set; }
        public decimal Price { get; set; }
        // public int MyProperty { get; set; }
    }

    public enum DiscountType
    {
        Percent = 1,

    }
}