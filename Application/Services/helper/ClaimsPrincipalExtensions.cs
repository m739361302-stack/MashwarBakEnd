using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services.helper
{
    public static class ClaimsPrincipalExtensions
    {
        public static long GetUserId(this ClaimsPrincipal user)
            => long.Parse(user.FindFirstValue(AppClaims.UserId) ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? "0");

        public static byte GetUserType(this ClaimsPrincipal user)
            => byte.Parse(user.FindFirstValue(AppClaims.UserType) ?? "0");

        public static byte GetApprovalStatus(this ClaimsPrincipal user)
            => byte.Parse(user.FindFirstValue(AppClaims.ApprovalStatus) ?? "0");

        public static bool IsActiveUser(this ClaimsPrincipal user)
            => (user.FindFirstValue(AppClaims.IsActive) ?? "0") == "1";

        public static string GetRole(this ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.Role) ?? user.FindFirstValue(AppClaims.Role) ?? "";
    }
}
