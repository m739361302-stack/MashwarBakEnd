using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Domain.Entities;

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

        public (string token, DateTime expiresAtUtc) CreateToken(User user)
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
    }
}
