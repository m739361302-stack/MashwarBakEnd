using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Claims;

namespace Application.Services.Security
{

    public static class ClaimsExtensions
    {
        public static long GetUserId(this ClaimsPrincipal user)
        {
            var v = user.FindFirstValue("userId") ?? user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
            return long.Parse(v!);
        }
    }

}
