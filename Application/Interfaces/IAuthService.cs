using Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);
        Task<ApiOkDto> RegisterDriverAsync(RegisterDriverRequestDto dto);
        Task<MeResponseDto> MeAsync(long userId);
    }
}
