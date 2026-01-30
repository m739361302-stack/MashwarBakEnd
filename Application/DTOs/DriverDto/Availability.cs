using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.DriverDto
{
    public class Availability
    {
        public record AvailabilitySlotDto(long Id, DateTime StartAt, DateTime EndAt, bool IsActive);

        public record CreateAvailabilitySlotDto(DateTime StartAt, DateTime EndAt);

        public record UpdateAvailabilitySlotDto(DateTime StartAt, DateTime EndAt, bool IsActive);

        public record DriverAvailabilityStateDto(bool IsAvailable);
        public record SetDriverAvailabilityDto(bool IsAvailable);

    }
}
