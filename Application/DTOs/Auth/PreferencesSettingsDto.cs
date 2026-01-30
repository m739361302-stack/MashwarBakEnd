using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Auth
{
    public class PreferencesSettingsDto
    {
        public string Language { get; set; } = "ar";      // ar/en
        public string Timezone { get; set; } = "Asia/Riyadh";
        public string Theme { get; set; } = "light";      // light/dark/system
    }
}
