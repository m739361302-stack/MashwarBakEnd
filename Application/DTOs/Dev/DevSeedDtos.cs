using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Dev
{
    public record SeedAdminRequestDto(
      string FullName,
      string Phone,
      string Password,
      string? Email = null
  );

}
