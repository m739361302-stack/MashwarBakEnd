using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.Security
{
    public class JwtOptions
    {
        public string Issuer { get; set; } = "Mashwar.Api";
        public string Audience { get; set; } = "Mashwar.Client";
        public string Key { get; set; } = null!; // put in appsettings
        public int ExpMinutes { get; set; } = 180;
    }
}
