using Application.DTOs.Auth;
using Application.DTOs.CustomerDtos;
using Application.Interfaces;
using Application.Services.Security;
using Domain.Entities;
using Domain.Enums;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
        private readonly IWebHostEnvironment _env;
        private readonly IFileStorage _fileStorage;
        public AuthService(MashwarDbContext db, IJwtTokenService jwt, IWebHostEnvironment env, IFileStorage fileStorage) {

            _db = db;
            _jwt = jwt;
            _env = env;
            _fileStorage = fileStorage;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
          
                var phone = dto.Phone.Trim();

                var user = await _db.Users.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Phone == phone);

                if (user == null || !PasswordHasher.Verify(dto.Password, user.PasswordHash))
                    //throw new InvalidOperationException("بيانات الدخول غير صحيحة");

                return new LoginResponseDto(
                     AccessToken: "",
                     ExpiresAtUtc:null ,
                     Issuccessful: false,
                     message: "بيانات الدخول غير صحيحة",
                     null
                 );

                if (!user.IsActive)
                    //throw new InvalidOperationException("الحساب غير مفعل");
                    return new LoginResponseDto(
                  AccessToken: "",
                  ExpiresAtUtc: null,
                  Issuccessful: false,
                  message: "الحساب غير مفعل",
                  null
              );

                // منع سائق غير معتمد من دخول لوحة السائق (لكن يسمح له دخول عام؟ هنا نمنع الدخول كليًا)
                if (user.UserType == UserType.Driver && user.ApprovalStatus != ApprovalStatus.Approved)
                    // throw new InvalidOperationException("حساب السائق قيد المراجعة ولم يتم اعتماده بعد");
                    //if (!user.IsActive)
                        //throw new InvalidOperationException("الحساب غير مفعل");
                        return new LoginResponseDto(
                      AccessToken: "",
                      ExpiresAtUtc: null,
                      Issuccessful: false,
                      message: "حساب السائق قيد المراجعة ولم يتم اعتماده بعد",
                      null
                  );

                // Update LastLoginAt (اختياري)
                await _db.Users.Where(x => x.Id == user.Id)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.LastLoginAt, DateTime.UtcNow));

                var (token, expiresAt) = _jwt.CreateToken(user);

                //return new LoginResponseDto(
                //    token,
                //    expiresAt,
                //    true,
                //    "تم جلب البيانات بنجاح",

                //    new AuthUserDto(user.Id, user.FullName, user.Phone, user.Email, (byte)user.UserType, (byte)user.ApprovalStatus, user.IsActive)
                //);
                return new LoginResponseDto(
                    AccessToken : token,
                    ExpiresAtUtc: expiresAt,
                    Issuccessful:true,
                    message:"تم جلب البيانات بنجاح",

                    new AuthUserDto(user.Id, user.FullName, user.Phone, user.Email, (byte)user.UserType, (byte)user.ApprovalStatus, user.IsActive)
                );
<<<<<<< HEAD
           
         
=======

            }
            catch (Exception ex )
            {

                return null;
            }
>>>>>>> a5fe3ccd2a3e181b99cd9d46821e8519a40f39df
            
        }

        public async Task<ApiOkDto> RegisterDriverAsync2(RegisterDriverRequestDto dto)
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

            var nationalUsed = await _db.Drivers.AnyAsync(d => d.NationalId == dto.NationalId);
            if (nationalUsed) throw new InvalidOperationException("رقم الهوية مستخدم مسبقًا");

            var licenseUsed = await _db.Drivers.AnyAsync(d => d.LicenseNumber == dto.LicenseNumber);
            if (licenseUsed) throw new InvalidOperationException("رقم الرخصة مستخدم مسبقًا");

            await using var tx = await _db.Database.BeginTransactionAsync();

            var user = new User
            {
                UserType = UserType.Driver,
                FullName = dto.FullName.Trim(),
                Phone = phone,
                Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
                PasswordHash = PasswordHasher.Hash(dto.Password),
                IsActive = true,
                ApprovalStatus = ApprovalStatus.Pending,
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
            await _db.SaveChangesAsync(); // ✅ لازم عشان driver.Id يطلع

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

            // ✅ حفظ الملفات (اختياري)
            await SaveDriverFilesIfAnyAsync(driver.UserId, dto);

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return new ApiOkDto("تم إرسال طلب تسجيل السائق بنجاح. سيتم مراجعته من الإدارة.");
        }


        //private async Task SaveDriverFilesIfAnyAsync(long driverId, RegisterDriverRequestDto dto)
        //{
        //    // folder: drivers/{driverId}
        //    var folder = Path.Combine("drivers", driverId.ToString()).Replace("\\", "/");

        //    async Task save(IFormFile? file, DriverDocumentType type, string baseName, CancellationToken ct = default)
        //    {
        //        if (file == null || file.Length == 0) return;

        //        // 1) حفظ على disk
        //        var (storedPath, fileName, contentType, sizeBytes) =
        //            await _fileStorage.SaveAsync(file, folder, baseName, ct);

        //        // 2) حفظ metadata بقاعدة البيانات (اختياري لكن مفيد)
        //        _db.DriverDocuments.Add(new DriverDocument
        //        {
        //            DriverUserId = driverId,
        //            DocType = type,
        //            FileName = fileName,
        //            StoredPath = storedPath,
        //            ContentType = contentType,
        //            SizeBytes = sizeBytes,
        //            Created = DateTime.Now
        //        });
        //    }

        //    // هنا ملفاتك
        //    await save(dto.NationalIdFile, DriverDocumentType.NationalId, "national_id");
        //    await save(dto.LicenseFile, DriverDocumentType.License, "license");
        //    await save(dto.CarRegFile, DriverDocumentType.CarRegistration, "car_reg");
        //}

        private async Task SaveDriverFilesIfAnyAsync(
            long driverUserId,
            RegisterDriverRequestDto dto,
            CancellationToken ct = default)
        {
            var folder = $"drivers/{driverUserId}";

            async Task save(IFormFile? file, DriverDocumentType type, string baseName)
            {
                if (file == null || file.Length == 0) return;

                var (storedPath, fileName, contentType, sizeBytes) =
                    await _fileStorage.SaveAsync(file, folder, baseName, ct);

                _db.DriverDocuments.Add(new DriverDocument
                {
                    DriverUserId = driverUserId,
                    DocType = type,
                    FileName = fileName,
                    StoredPath = storedPath,
                    ContentType = contentType,
                    SizeBytes = sizeBytes,
                    Created = DateTime.UtcNow
                });
            }

            await save(dto.NationalIdFile, DriverDocumentType.NationalId, "national_id");
            await save(dto.LicenseFile, DriverDocumentType.License, "license");
            await save(dto.CarRegFile, DriverDocumentType.CarRegistration, "car_registration");
        }


        public async Task<RegisterCustomerResponse> RegisterCustomerAsyncold(RegisterCustomerRequest request, CancellationToken ct)
        {
            var phone = NormalizePhone(request.Phone);
            var fullName = (request.FullName ?? "").Trim();
            var password = request.Password ?? "";

            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Phone is required.");

            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("FullName is required.");

            if (password.Length < 6)
                throw new ArgumentException("Password must be at least 6 characters.");

            var exists = await _db.Users.AsNoTracking()
                .AnyAsync(x => x.Phone == phone, ct);

            if (exists)
                throw new InvalidOperationException("Phone already exists.");

            var user = new User
            {
                Phone = phone,
                FullName = fullName,
                UserType = UserType.Customer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            user.PasswordHash = PasswordHasher.Hash(password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);

            return new RegisterCustomerResponse
            {
                UserId = user.Id,
                Message = "Customer registered successfully."
            };
        }

        public async Task<RegisterCustomerResponse> RegisterCustomerAsync(RegisterCustomerRequest request, CancellationToken ct)
        {
            var phone = NormalizePhone(request.Phone);
            var fullName = (request.FullName ?? "").Trim();
            var password = request.Password ?? "";

            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Phone is required.");

            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("FullName is required.");

            if (password.Length < 6)
                throw new ArgumentException("Password must be at least 6 characters.");

            var exists = await _db.Users.AsNoTracking()
                .AnyAsync(x => x.Phone == phone, ct);

            if (exists)
                throw new InvalidOperationException("Phone already exists.");

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            var user = new User
            {
                Phone = phone,
                FullName = fullName,
                UserType = UserType.Customer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = PasswordHasher.Hash(password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct); // هنا صار عندك user.Id

            var customer = new Customer
            {
                UserId = user.Id,
                //CityId = request.CityId,          // لو موجودة في RegisterCustomerRequest
                //AvatarUrl = request.AvatarUrl     // لو موجودة في RegisterCustomerRequest
            };

            _db.Customers.Add(customer);
            await _db.SaveChangesAsync(ct);

            await tx.CommitAsync(ct);

            return new RegisterCustomerResponse
            {
                UserId = user.Id,
                Message = "Customer registered successfully."
            };
        }

        private static string NormalizePhone(string? phone)
        {
            var p = (phone ?? "").Trim();

            // مثال تطبيع بسيط: إزالة المسافات والـ+ إن وجدت
            p = p.Replace(" ", "").Replace("-", "");

            // إذا تحب قواعد دقيقة (مثل يبدأ بـ 05 أو 7...) نضيفها حسب بلدك
            return p;
        }
        public async Task<MeResponseDto> MeAsync(long userId)
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null) throw new InvalidOperationException("المستخدم غير موجود");

            return new MeResponseDto(
                new AuthUserDto(user.Id, user.FullName, user.Phone, user.Email, (byte)user.UserType, (byte)user.ApprovalStatus, user.IsActive)
            );
        }

        public async Task<CustomerProfileResponse> UpdateProfileAsync(
        long userId,
        UpdateCustomerProfileRequest request,
        CancellationToken ct)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(x => x.Id == userId, ct);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            if (!user.IsActive)
                throw new InvalidOperationException("User is not active.");

            // تحقق بسيط
            request.FullName = request.FullName.Trim();
            request.Phone = request.Phone.Trim();

            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new ArgumentException("Full name is required.");

            if (string.IsNullOrWhiteSpace(request.Phone))
                throw new ArgumentException("Phone is required.");

            // تحديث
            user.FullName = request.FullName;
            user.Phone = request.Phone;
            user.Email = request.Email?.Trim();

            await _db.SaveChangesAsync(ct);

            return new CustomerProfileResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Phone = user.Phone,
                Email = user.Email,
                IsActive = user.IsActive
            };
        }

        public async Task<ChangePasswordResponse> ChangePasswordAsync(long userId, ChangePasswordRequest request, CancellationToken ct)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId, ct)
                       ?? throw new KeyNotFoundException("User not found.");

            if (!user.IsActive)
                throw new InvalidOperationException("User is not active.");

            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
                throw new ArgumentException("CurrentPassword is required.");

            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
                throw new ArgumentException("NewPassword must be at least 6 characters.");

            // verify current password
            var verify = PasswordHasher.Verify(user.PasswordHash, request.CurrentPassword);
            if (!verify)
                throw new InvalidOperationException("Current password is incorrect.");

            // update hash
            user.PasswordHash = PasswordHasher.Hash(request.NewPassword);

            await _db.SaveChangesAsync(ct);

            return new ChangePasswordResponse { Success = true, Message = "Password changed successfully." };
        }

  
        private async Task SaveDriverFilesAsync(long driverId, RegisterDriverRequest request, CancellationToken ct)
        {
            var root = Path.Combine(_env.ContentRootPath, "uploads", "drivers", driverId.ToString());
            Directory.CreateDirectory(root);

            async Task save(IFormFile? file, string name)
            {
                if (file == null || file.Length == 0) return;

                var ext = Path.GetExtension(file.FileName);
                var safeName = $"{name}{ext}";
                var path = Path.Combine(root, safeName);

                using var stream = File.Create(path);
                await file.CopyToAsync(stream, ct);

                // (اختياري) خزّن metadata في DB لو عندك DriverDocuments table
            }

            await save(request.NationalIdFile, "national_id");
            await save(request.LicenseFile, "license");
            await save(request.CarRegFile, "car_reg");
        }
    }
}

