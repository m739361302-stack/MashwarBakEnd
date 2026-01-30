using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.DriverService
{
    using Application.DTOs.Result;
    using Application.Interfaces.IDriver;
    using Domain.Entities;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using static Application.DTOs.DriverDto.Availability;

    public class DriverAvailabilityService : IDriverAvailabilityService
    {
        private readonly MashwarDbContext _db;
        public DriverAvailabilityService(MashwarDbContext db) => _db = db;

        private async Task<Driver?> GetDriverByUserIdAsync(long userId, CancellationToken ct)
            => await _db.Drivers.IgnoreQueryFilters().FirstOrDefaultAsync(d => d.UserId == userId, ct);

        public async Task<ApiResult<DriverAvailabilityStateDto>> GetDriverAvailabilityAsync(long userId, CancellationToken ct)
        {
            var driver = await GetDriverByUserIdAsync(userId, ct);
            if (driver is null) return ApiResult<DriverAvailabilityStateDto>.NotFound("Driver not found.");

            return ApiResult<DriverAvailabilityStateDto>.Ok(new DriverAvailabilityStateDto(driver.IsAvailable));
        }

        public async Task<ApiResult<List<AvailabilitySlotDto>>> GetSlotsAsync(long userId, CancellationToken ct)
        {
            var driver = await GetDriverByUserIdAsync(userId, ct);
            if (driver is null) return ApiResult<List<AvailabilitySlotDto>>.NotFound("Driver not found.");

            var slots = await _db.DriverAvailabilitySlots.AsNoTracking()
                .Where(s => s.DriverUserId == driver.UserId)
                .OrderBy(s => s.StartAt)
                .Select(s => new AvailabilitySlotDto(s.Id, s.StartAt, s.EndAt, s.IsActive))
                .ToListAsync(ct);

            return ApiResult<List<AvailabilitySlotDto>>.Ok(slots);
        }

        public async Task<ApiResult<long>> CreateSlotAsync(long userId, CreateAvailabilitySlotDto dto, CancellationToken ct)
        {
            var driver = await GetDriverByUserIdAsync(userId, ct);
            if (driver is null) return ApiResult<long>.NotFound("السائق غير موجود.");

            if (dto.EndAt <= dto.StartAt)
                return ApiResult<long>.Fail("يجب ان يكون تاريخ النهاية اكبر من تاريخ البداية");

            if (dto.StartAt < DateTime.UtcNow.AddMinutes(-1))
                return ApiResult<long>.Fail("يجب ان يكون التاريخ مستقبلا على الاقل اكبر من تاريخ اليوم ");

            // منع تداخل الفترات (اختياري لكنه مفيد)
            var overlap = await _db.DriverAvailabilitySlots
                .AnyAsync(s => s.DriverUserId == driver.UserId && s.IsActive &&
                               dto.StartAt < s.EndAt && dto.EndAt > s.StartAt, ct);

            if (overlap)
                return ApiResult<long>.Fail("تداخل في اضافة التوافر مع فترة سابقة");

            var slot = new DriverAvailabilitySlot
            {
                DriverUserId = driver.UserId,
                StartAt = dto.StartAt,
                EndAt = dto.EndAt,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.DriverAvailabilitySlots.Add(slot);
            await _db.SaveChangesAsync(ct);

            return ApiResult<long>.Ok(slot.Id, "Slot created.");
        }

        public async Task<ApiResult> DeleteSlotAsync(long userId, int slotId, CancellationToken ct)
        {
            var driver = await GetDriverByUserIdAsync(userId, ct);
            if (driver is null) return ApiResult.NotFound("Driver not found.");

            var slot = await _db.DriverAvailabilitySlots.FirstOrDefaultAsync(s => s.Id == slotId && s.DriverUserId == driver.UserId, ct);
            if (slot is null) return ApiResult.NotFound("Slot not found.");

            _db.DriverAvailabilitySlots.Remove(slot);
            await _db.SaveChangesAsync(ct);

            return ApiResult.Ok("Slot deleted.");
        }

        public async Task<ApiResult> UpdateSlotAsync(long userId, int slotId, UpdateAvailabilitySlotDto dto, CancellationToken ct)
        {
            var driver = await GetDriverByUserIdAsync(userId, ct);
            if (driver is null) return ApiResult.NotFound("Driver not found.");

            var slot = await _db.DriverAvailabilitySlots.FirstOrDefaultAsync(s => s.Id == slotId && s.DriverUserId == driver.UserId, ct);
            if (slot is null) return ApiResult.NotFound("Slot not found.");

            //if (dto.EndAt <= dto.StartAt)
            //    return ApiResult.Fail("EndAt must be greater than StartAt.");

            // منع تداخل (مع استثناء نفس السجل)
            var overlap = await _db.DriverAvailabilitySlots
                .AnyAsync(s => s.DriverUserId == driver.UserId && s.Id != slotId && s.IsActive &&
                               dto.StartAt < s.EndAt && dto.EndAt > s.StartAt, ct);

            if (overlap)
                return ApiResult.Fail("Availability slot overlaps with an existing slot.");

            //slot.StartAt = dto.StartAt;
            //slot.EndAt = dto.EndAt;
            slot.IsActive = dto.IsActive;
            //slot.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return ApiResult.Ok("Slot updated.");
        }

        public async Task<ApiResult> SetDriverAvailabilityAsync(long userId, SetDriverAvailabilityDto dto, CancellationToken ct)
        {
            var driver = await GetDriverByUserIdAsync(userId, ct);
            if (driver is null) return ApiResult.NotFound("Driver not found.");

            driver.IsAvailable = dto.IsAvailable;
            //driver.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return ApiResult.Ok("Driver availability updated.");
        }
    }

}
