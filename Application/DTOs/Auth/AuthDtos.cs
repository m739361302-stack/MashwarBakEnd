using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Auth
{
    public record LoginRequestDto(string Phone, string Password);

    public record AuthUserDto(
        long Id,
        string FullName,
        string Phone,
        string? Email,
        byte UserType,
        byte ApprovalStatus,
        bool IsActive
    );

    public record LoginResponseDto(
        string AccessToken,
        DateTime? ExpiresAtUtc,
        bool Issuccessful,
        string message,
        AuthUserDto? User
    );
    public record LoginResponseErrorDto(

    bool Issuccessful,
    string message
 
);


    public record MeResponseDto(AuthUserDto User);

    public record RegisterDriverRequestDto(
        string FullName,
        string Phone,
        string? Email,
        string Password,
        int? CityId,

        // Driver fields
        string NationalId,
        string LicenseNumber,
        DateOnly? LicenseExpiry,
        string? Iban,

        // Vehicle (optional)
        string? CarMake,
        string? CarModel,
        short? CarYear,
        string? PlateNumber,
        string? CarColor
    );

    public record ApiOkDto(string Message);

}
