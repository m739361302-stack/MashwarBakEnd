using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.CustomerDtos
{
    public class NotificationsSettingsDto
    {
        public bool NotifyBookingUpdates { get; set; }
        public bool NotifyTripReminders { get; set; }
        public bool NotifyPromotions { get; set; }
        public bool NotifyEmail { get; set; }
        public bool NotifySms { get; set; }
    }
}
