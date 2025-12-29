using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Services.Security;
using Domain.Entities;
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
            try
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
            catch (Exception ex )
            {

                return null;
            }
            
        }

        public async Task<ApiOkDto> RegisterDriverAsync(RegisterDriverRequestDto dto)
        {
            var phone = dto.Phone.Trim();

            var exists = await _db.Users.AnyAsync(x => x.Phone == phone);
            if (exists) throw new InvalidOperationException("رقم الجوال مستخدم مسبقًا");

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var emailExists = await _db.Users.AnyAsync(x => x.Email == dto.Email);
                if (emailExists) throw new InvalidOperationException("البريد الإلكتروني مستخدم مسبقًا");
            }

            // (اختياري) uniqueness for nationalId/license - لو فعلتها بقاعدة البيانات
            var nationalUsed = await _db.Drivers.AnyAsync(d => d.NationalId == dto.NationalId);
            if (nationalUsed) throw new InvalidOperationException("رقم الهوية مستخدم مسبقًا");

            var licenseUsed = await _db.Drivers.AnyAsync(d => d.LicenseNumber == dto.LicenseNumber);
            if (licenseUsed) throw new InvalidOperationException("رقم الرخصة مستخدم مسبقًا");

            using var tx = await _db.Database.BeginTransactionAsync();

            var user = new User
            {
                UserType = UserType.Driver,
                FullName = dto.FullName.Trim(),
                Phone = phone,
                Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
                PasswordHash = PasswordHasher.Hash(dto.Password),
                IsActive = true,
                ApprovalStatus = ApprovalStatus.Pending, // ✅ Pending
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var driver = new Driver
            {
                UserId = user.Id,
                CityId = dto.CityId,
                IsAvailable = false,
                NationalId = dto.NationalId.Trim(),
                LicenseNumber = dto.LicenseNumber.Trim(),
                LicenseExpiry = dto.LicenseExpiry,
                Iban = string.IsNullOrWhiteSpace(dto.Iban) ? null : dto.Iban.Trim(),
                RatingAvg = 0,
                RatingCount = 0
            };
            _db.Drivers.Add(driver);

            // Vehicle optional
            if (!string.IsNullOrWhiteSpace(dto.CarMake) ||
                !string.IsNullOrWhiteSpace(dto.CarModel) ||
                !string.IsNullOrWhiteSpace(dto.PlateNumber))
            {
                _db.DriverVehicles.Add(new DriverVehicle
                {
                    DriverUserId = user.Id,
                    Make = dto.CarMake?.Trim(),
                    Model = dto.CarModel?.Trim(),
                    Year = dto.CarYear,
                    PlateNumber = dto.PlateNumber?.Trim(),
                    Color = dto.CarColor?.Trim(),
                    IsPrimary = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return new ApiOkDto("تم إرسال طلب تسجيل السائق بنجاح. سيتم مراجعته من الإدارة.");
        }


        public async Task<MeResponseDto> MeAsync(long userId)
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null) throw new InvalidOperationException("المستخدم غير موجود");

            return new MeResponseDto(
                new AuthUserDto(user.Id, user.FullName, user.Phone, user.Email, (byte)user.UserType, (byte)user.ApprovalStatus, user.IsActive)
            );
        }
    }
}
