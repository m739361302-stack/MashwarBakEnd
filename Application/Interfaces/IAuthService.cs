using Application.DTOs.Auth;
using Application.DTOs.CustomerDtos;
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

        Task<RegisterCustomerResponse> RegisterCustomerAsync(RegisterCustomerRequest request, CancellationToken ct);

        Task<CustomerProfileResponse> UpdateProfileAsync(
       long userId,
       UpdateCustomerProfileRequest request,
       CancellationToken ct);

        Task<ChangePasswordResponse> ChangePasswordAsync(long userId, ChangePasswordRequest request, CancellationToken ct);
      


    }
}
