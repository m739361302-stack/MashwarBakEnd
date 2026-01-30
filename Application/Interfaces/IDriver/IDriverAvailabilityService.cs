using Application.DTOs.Result;
using System;
using System.Collections.Generic;
using System.Text;
using static Application.DTOs.DriverDto.Availability;

namespace Application.Interfaces.IDriver
{
    public interface IDriverAvailabilityService
    {
        Task<ApiResult<DriverAvailabilityStateDto>> GetDriverAvailabilityAsync(long userId, CancellationToken ct);

        Task<ApiResult<List<AvailabilitySlotDto>>> GetSlotsAsync(long userId, CancellationToken ct);
        Task<ApiResult<long>> CreateSlotAsync(long userId, CreateAvailabilitySlotDto dto, CancellationToken ct);
        Task<ApiResult> DeleteSlotAsync(long userId, int slotId, CancellationToken ct);
        Task<ApiResult> UpdateSlotAsync(long userId, int slotId, UpdateAvailabilitySlotDto dto, CancellationToken ct); // optional

        Task<ApiResult> SetDriverAvailabilityAsync(long userId, SetDriverAvailabilityDto dto, CancellationToken ct);
    }
}
