using System;
using System.Collections.Generic;
using System.Text;
using static Domain.Entities.Mashwar;

namespace Domain.Entities
{
    internal class UserSettings
    {
        public long UserId { get; set; }
        public string Language { get; set; } = "ar";
        public string Theme { get; set; } = "light";
        public string Timezone { get; set; } = "Asia/Riyadh";
        public bool NotifyBookingUpdates { get; set; } = true;
        public bool NotifyTripReminders { get; set; } = true;
        public bool NotifyPromotions { get; set; } = false;
        public bool NotifySms { get; set; } = true;
        public bool NotifyEmail { get; set; } = false;
        public DateTime UpdatedAt { get; set; }

        public User User { get; set; } = null!;
    }
}
