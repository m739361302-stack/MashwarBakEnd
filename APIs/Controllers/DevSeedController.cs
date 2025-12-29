using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Route("api/v1/dev/seed")]
    public class DevSeedController : ControllerBase
    {
        private readonly MashwarDbContext _db;
        private readonly IWebHostEnvironment _env;

        public DevSeedController(MashwarDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        /// <summary>
        /// DEV ONLY: Create/Upsert an Admin user with hashed password.
        /// {
        //  "fullName": "Admin1",
        //  "phone": "0500000000",
        //  "password": "Admin@12345",
        //  "email": "admin@demo.sa"
        //}
        /// </summary>
        [HttpPost("admin")]
        public async Task<IActionResult> SeedAdmin([FromBody] SeedAdminRequestDto dto)
        {
            try
            {
                if (!_env.IsDevelopment())
                    return NotFound(); // يخفيه بالكامل خارج بيئة التطوير

                if (string.IsNullOrWhiteSpace(dto.FullName) ||
                    string.IsNullOrWhiteSpace(dto.Phone) ||
                    string.IsNullOrWhiteSpace(dto.Password))
                {
                    return BadRequest(new { message = "FullName/Phone/Password مطلوبين" });
                }

                var phone = dto.Phone.Trim();
                var email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();

                // لو email موجود تأكد ما يتعارض مع مستخدم آخر
                if (email != null)
                {
                    var emailUsed = await _db.Users.AnyAsync(u => u.Email == email && u.Phone != phone);
                    if (emailUsed) return BadRequest(new { message = "Email مستخدم مسبقًا" });
                }

                var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == phone);

                if (user == null)
                {
                    user = new User
                    {
                        UserType = UserType.Admin,
                        FullName = dto.FullName.Trim(),
                        Phone = phone,
                        Email = email,
                        PasswordHash = PasswordHasher.Hash(dto.Password),
                        IsActive = true,
                        ApprovalStatus = ApprovalStatus.Approved,
                        CreatedAt = DateTime.UtcNow
                    };

                    _db.Users.Add(user);
                    await _db.SaveChangesAsync();

                    return Ok(new
                    {
                        message = "تم إنشاء Admin بنجاح (DEV).",
                        adminUserId = user.Id,
                        phone = user.Phone
                    });
                }

                // Upsert (تحديث)
                user.UserType = UserType.Admin;
                user.FullName = dto.FullName.Trim();
                user.Email = email;
                user.IsActive = true;
                user.ApprovalStatus = ApprovalStatus.Approved;
                user.PasswordHash = PasswordHasher.Hash(dto.Password);
                user.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                return Ok(new
                {
                    message = "تم تحديث Admin بنجاح (DEV).",
                    adminUserId = user.Id,
                    phone = user.Phone
                });
            }
            catch (Exception ex)
            {

                throw;
            }
           
        }
    }
}
