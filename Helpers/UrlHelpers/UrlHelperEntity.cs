using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCoursePlatform.Helpers.UrlHelpers
{
    [Table("UrlHelper")]
    public class UrlHelperEntity
    {
        [Key]
        public int Id { get; set; }
        public string? ConfirmEmailUrl { get; set; }
        public string? ResetPasswordUrl { get; set; }
    }
}