using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Data.Entities
{
    public class UserRefreshToken
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")]
        [InverseProperty("UserRefreshTokens")]
        public virtual AppUser User { get; set; } = null!;
        public DateTime Expires { get; set; }
        public bool Active => DateTime.UtcNow <= Expires;
        public string? RemoteIpAddress { get; set; }
    }
}