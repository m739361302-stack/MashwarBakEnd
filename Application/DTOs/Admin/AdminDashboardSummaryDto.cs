using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Admin
{
    public record AdminDashboardSummaryDto(
      int PendingBookings,
      int ActiveTrips,
      int InactiveDrivers
  );
}
