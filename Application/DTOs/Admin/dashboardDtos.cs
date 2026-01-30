using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Admin
{
    public class dashboardDtos
    {
        public sealed record AdminDashboardDto(
            AdminDashboardKpisDto Kpi,
            List<AdminDashboardBookingDto> LatestBookings,
            List<AdminDashboardDriverDto> LatestDrivers
        );

        public sealed record AdminDashboardKpisDto(
            int TotalDrivers,
            int ActiveDrivers,
            int ActiveTrips,
            int PendingBookings,
            decimal RevenueToday
        );

        public sealed record AdminDashboardBookingDto(
            long Id,
            string From,
            string To,
            decimal Price,
            string Status,
            DateTime CreatedAtUtc
        );

        public sealed record AdminDashboardDriverDto(
            long Id,
            string Name,
            string City,
            bool IsActive,
            bool IsAvailable
        );
    }
}
