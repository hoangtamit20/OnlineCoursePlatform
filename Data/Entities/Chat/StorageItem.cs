using System.ComponentModel.DataAnnotations;

namespace OnlineCoursePlatform.Data.Entities.Chat
{
    public class StorageItem
    {
        [Key]
        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public long Discount { get; set; }
    }
}