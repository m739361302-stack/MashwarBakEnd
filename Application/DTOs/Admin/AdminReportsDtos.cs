using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Admin
{
    public class AdminReportsDtos
    {
        public sealed record AdminReportsResponse(
            AdminReportsKpisDto Kpis,
            List<LabelValueDto> SummaryRows,
            List<AdminBookingRowDto> BookingsRows,
            List<AdminDriverRowDto> DriverRows,
            List<AdminTripRowDto> TripRows,
            List<AdminRevenueRowDto> RevenueRows
        );

        public sealed record AdminReportsKpisDto(
            int Bookings,
            int Confirmed,
            int Cancelled,
            int DriversActive,
            int TripsActive,
            decimal Revenue
        );

        public sealed record LabelValueDto(string Label, object? Value);

        public sealed record AdminBookingRowDto(
            long Id,
            string From,
            string To,
            decimal Price,
            string Status,
            string Driver,
            string Customer,
            DateTime CreatedAtUtc
        );

        public sealed record AdminDriverRowDto(
            long Id,
            string Name,
            string City,
            int ActiveTrips,
            int TotalTrips,
            double Rating,
            bool IsActive,
            bool IsAvailable
        );

        public sealed record AdminTripRowDto(
            long Id,
            string From,
            string To,
            DateTime DepartAtUtc,
            decimal Price,
            string Driver,
            bool IsActive
        );

        public sealed record AdminRevenueRowDto(
            DateOnly Day,
            decimal Revenue,
            int Bookings
        );

        public sealed class AdminReportsQuery2
        {
            // today | last7 | last30 | custom
            public string Range { get; set; } = "last7";

            public DateOnly? FromDate { get; set; }
            public DateOnly? ToDate { get; set; }

            // all | PendingDriverConfirm | Confirmed | RejectedByDriver | CancelledByCustomer
            public string Status { get; set; } = "all";

            public string FromCity { get; set; } = "all";
            public string ToCity { get; set; } = "all";
        }

        public sealed class AdminReportsQuery
        {
            public string Range { get; set; } = "last7"; // today | last7 | last30 | custom
            public DateOnly? FromDate { get; set; }
            public DateOnly? ToDate { get; set; }

            public string Status { get; set; } = "all"; // all | PendingDriverConfirm | Confirmed ...

            public string FromCity { get; set; } = "all"; // city name
            public string ToCity { get; set; } = "all";   // city name
        }
    }
}
