using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Data.Entities
{
    public class UserRefreshToken
    {
        [Key]
        public int Id { get; set; }
        public string RefreshToken { get; set; } = Guid.NewGuid().ToString();
        [StringLength(2000)]
        public string AccessToken { get; set; } = null!;
        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")]
        [InverseProperty("UserRefreshTokens")]
        public virtual AppUser User { get; set; } = null!;
        public DateTime Expires { get; set; }
        public bool Active => DateTime.UtcNow <= Expires;
        public DateTime LastRevoked { get; set; }
        public bool IsRevoked { get; set; } = false;
        public string? RemoteIpAddress { get; set; }
    }
}