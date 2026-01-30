using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services.Security
{
    public interface IJwtTokenService
    {
        (string token, DateTime expiresAtUtc) CreateToken(User user);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _opt;

        public JwtTokenService(IOptions<JwtOptions> opt)
        {
            _opt = opt.Value;
        }



        public (string token, DateTime expiresAtUtc) CreateToken2(User user)
        {
            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(_opt.ExpMinutes);

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("uid", user.Id.ToString()),
            new Claim("userType", ((byte)user.UserType).ToString()),
            new Claim("approvalStatus", ((byte)user.ApprovalStatus).ToString()),
            new Claim("isActive", user.IsActive ? "1" : "0"),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _opt.Issuer,
                audience: _opt.Audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expires);
        }

        public (string token, DateTime expiresAtUtc) CreateToken(User user)
        {
            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(_opt.ExpMinutes);

            // ✅ Role text (اختياري لكن مفيد)
            // عدّلها حسب enum عندك
            var role =
                user.UserType == UserType.Admin ? "Admin" :
                user.UserType == UserType.Driver ? "Driver" :
                "Customer";

            var claims = new List<Claim>
    {
        // ===== Required / Standard =====
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),

        // ===== App Core Claims (ثابتة) =====
        new Claim("uid", user.Id.ToString()),
        new Claim("userType", ((byte)user.UserType).ToString()),               // 1/2/3...
        new Claim("approvalStatus", ((byte)user.ApprovalStatus).ToString()),   // 0/1/2...
        new Claim("isActive", user.IsActive ? "1" : "0"),

        // ===== Identity / UI Friendly (اختياري) =====
        new Claim("name", user.FullName ?? ""),
        new Claim("phone", user.Phone ?? ""),

        // ===== Role for [Authorize(Roles=...)] =====
        new Claim(ClaimTypes.Role, role),
        new Claim("role", role),
    };

            // ===== Optional: CityId / OrgId حسب بنية كيانك =====
            // إذا كان عندك Customer.CityId أو Driver.CityId…
            // ملاحظة: استخدم ?. لتفادي null
            if (user.UserType == UserType.Customer && user.Customer?.CityId != null)
                claims.Add(new Claim("cityId", user.Customer.CityId.Value.ToString()));

            if (user.UserType == UserType.Driver && user.Driver?.CityId != null)
                claims.Add(new Claim("cityId", user.Driver.CityId.Value.ToString()));

            // لو عندك OrgId / TenantId
            // if (user.OrgId != null) claims.Add(new Claim("orgId", user.OrgId.ToString()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _opt.Issuer,
                audience: _opt.Audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expires);
        }
    }
}
