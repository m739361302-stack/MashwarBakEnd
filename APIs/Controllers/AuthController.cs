using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

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
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost("register-driver")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiOkDto>> RegisterDriver([FromBody] RegisterDriverRequestDto dto)
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
    }
}
