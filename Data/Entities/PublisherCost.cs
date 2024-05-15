using System.ComponentModel.DataAnnotations;
using OnlineCoursePlatform.Data.Entities.Chat;

namespace OnlineCoursePlatform.Data.Entities
{
    public class PublisherCost
    {
        [Key]
        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public int PublisherId { get; set; }
        public AppUser Publisher { get; set; } = null!;
        public string StorageItemId { get; set; } = null!;
        public StorageItem StorageItem { get; set; } = null!; 
        public long FileSizeStorage { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime ExpireDate { get; set; }

    }
}