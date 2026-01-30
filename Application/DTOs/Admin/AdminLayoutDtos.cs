using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Admin
{
    public sealed record AdminLayoutDto(
      AdminLayoutUserDto Admin,
      AdminLayoutCountersDto Counters
  );

    public sealed record AdminLayoutUserDto(
        long Id,
        string Name,
        string Role
    );

    public sealed record AdminLayoutCountersDto(
        int PendingBookings,
        int ActiveTrips,
        int InactiveDrivers
    );

}
