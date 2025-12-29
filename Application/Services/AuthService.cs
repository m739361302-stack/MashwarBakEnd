using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Services.Security;
using Domain.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class AuthService: IAuthService
    {
        private readonly MashwarDbContext _db;
        private readonly IJwtTokenService _jwt;
        public AuthService(MashwarDbContext db, IJwtTokenService jwt) {

            _db = db;
            _jwt = jwt;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var phone = dto.Phone.Trim();

            var user = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Phone == phone);

            if (user == null || !PasswordHasher.Verify(dto.Password, user.PasswordHash))
                throw new InvalidOperationException("بيانات الدخول غير صحيحة");

            if (!user.IsActive)
                throw new InvalidOperationException("الحساب غير مفعل");

            // منع سائق غير معتمد من دخول لوحة السائق (لكن يسمح له دخول عام؟ هنا نمنع الدخول كليًا)
            if (user.UserType == UserType.Driver && user.ApprovalStatus != ApprovalStatus.Approved)
                throw new InvalidOperationException("حساب السائق قيد المراجعة ولم يتم اعتماده بعد");

            // Update LastLoginAt (اختياري)
            await _db.Users.Where(x => x.Id == user.Id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.LastLoginAt, DateTime.UtcNow));

            var (token, expiresAt) = _jwt.CreateToken(user);

            return new LoginResponseDto(
                token,
                expiresAt,
                new AuthUserDto(user.Id, user.FullName, user.Phone, user.Email, (byte)user.UserType, (byte)user.ApprovalStatus, user.IsActive)
            );
        }

    }
}
