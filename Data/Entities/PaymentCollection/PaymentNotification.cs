using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OnlineCoursePlatform.Data.Entities.PaymentCollection
{
    /// <summary>
    /// 
    /// </summary>
    [Table("PaymentNotification")]
    public partial class PaymentNotification
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [StringLength(50)]
        public string? PaymentRefId { get; set; }

        [StringLength(50)]
        public string? NotificationDate { get; set; }

        [StringLength(50)]
        public string? NotificationAmount { get; set; }

        [StringLength(50)]
        public string? NotificationContent { get; set; }

        [StringLength(50)]
        public string? NotificationMessage { get; set; }

        [StringLength(50)]
        public string? NotificationSignature { get; set; }

        [StringLength(50)]
        public string? NotificationStatus { get; set; }

        [StringLength(50)]
        public string? NotificationResponseDate { get; set; }

        public string PaymentId { get; set; } = null!;

        [ForeignKey("PaymentId")]
        [InverseProperty("PaymentNotifications")]
        public virtual Payment Payment { get; set; } = null!;
    }

}