using Application.DTOs.Auth;
using Application.DTOs.CustomerDtos;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APIs.Controllers
{
    [Route("api/[controller]")]
    //[ApiController]

    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        public AuthController(IAuthService auth)
        {
            _auth = auth;
                
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto dto)
        {
            try
            {
                var res = await _auth.LoginAsync(dto);
                return Ok(res);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

          
        }


        [HttpPost("register-driver2")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiOkDto>> RegisterDriver2([FromBody] RegisterDriverRequestDto dto)
        {
            try
            {
                var res = await _auth.RegisterDriverAsync(dto);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("register-customer")]
        [AllowAnonymous]
        public async Task<ActionResult<RegisterCustomerResponse>> RegisterCustomer(
             [FromBody] RegisterCustomerRequest request,
             CancellationToken ct)
                    {
                        try
                        {
                            var res = await _auth.RegisterCustomerAsync(request, ct);
                            return Ok(res);
                        }
                        catch (InvalidOperationException ex)
                        {
                            // موجود مسبقًا
                            return Conflict(new { message = ex.Message });
                        }
                        catch (ArgumentException ex)
                        {
                            return BadRequest(new { message = ex.Message });
                        }
              }


        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<MeResponseDto>> Me()
        {
            var userIdStr = User.FindFirstValue("uid");
            if (!long.TryParse(userIdStr, out var userId))
                return Unauthorized(new { message = "Invalid token" });

            try
            {
                var res = await _auth.MeAsync(userId);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(
        [FromBody] UpdateCustomerProfileRequest request,
        CancellationToken ct)
        {
            // UserId من التوكن
            var userIdStr = User.FindFirstValue("uid")
                         ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!long.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var result = await _auth.UpdateProfileAsync(userId, request, ct);
            return Ok(result);
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out var userId)) return Unauthorized();

            var result = await _auth.ChangePasswordAsync(userId, req, ct);
            return Ok(result);
        }

        [HttpPost("register-driver")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> RegisterDriver([FromForm] RegisterDriverRequestDto dto, CancellationToken ct)
        {
            try
            {
                var res = await _auth.RegisterDriverAsync(dto); // أو مرر ct لو عندك
                return Ok(res);
            }
            catch (InvalidOperationException ex)
            {
                // موجود مسبقًا
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

     
        }
    }
}
