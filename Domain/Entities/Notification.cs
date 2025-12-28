using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Notification
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public NotificationChannel Channel { get; set; }
        public string? Title { get; set; }
        public string Body { get; set; } = null!;
        public string? RelatedType { get; set; }
        public long? RelatedId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
    }
}
