using Application.DTOs.Auth;
using Application.DTOs.CustomerDtos;
using Application.Interfaces.ICustomer;
using Domain.Entities;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.CustomerService
{
    public class UserSettingsService : IUserSettingsService
    {
        private readonly MashwarDbContext _db;

        public UserSettingsService(MashwarDbContext db)
        {
            _db = db;
        }

        private async Task<UserSettings> GetOrCreateAsync(long userId, CancellationToken ct)
        {
            var userExists = await _db.Users.AnyAsync(x => x.Id == userId, ct);
            if (!userExists) throw new KeyNotFoundException("User not found.");

            var settings = await _db.UserSettings.FirstOrDefaultAsync(x => x.UserId == userId, ct);
            if (settings != null) return settings;

            settings = new UserSettings { UserId = userId };
            _db.UserSettings.Add(settings);
            await _db.SaveChangesAsync(ct);
            return settings;
        }

        public async Task<NotificationsSettingsDto> GetNotificationsAsync(long userId, CancellationToken ct)
        {
            var s = await GetOrCreateAsync(userId, ct);
            return new NotificationsSettingsDto
            {
                NotifyBookingUpdates = s.NotifyBookingUpdates,
                NotifyTripReminders = s.NotifyTripReminders,
                NotifyPromotions = s.NotifyPromotions,
                NotifyEmail = s.NotifyEmail,
                NotifySms = s.NotifySms
            };
        }

        public async Task<NotificationsSettingsDto> UpdateNotificationsAsync(long userId, NotificationsSettingsDto dto, CancellationToken ct)
        {
            var s = await GetOrCreateAsync(userId, ct);

            s.NotifyBookingUpdates = dto.NotifyBookingUpdates;
            s.NotifyTripReminders = dto.NotifyTripReminders;
            s.NotifyPromotions = dto.NotifyPromotions;
            s.NotifyEmail = dto.NotifyEmail;
            s.NotifySms = dto.NotifySms;
            //s.UpdatedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return await GetNotificationsAsync(userId, ct);
        }

        public async Task<PreferencesSettingsDto> GetPreferencesAsync(long userId, CancellationToken ct)
        {
            var s = await GetOrCreateAsync(userId, ct);
            return new PreferencesSettingsDto
            {
                Language = s.Language,
                Timezone = s.Timezone,
                Theme = s.Theme
            };
        }

        public async Task<PreferencesSettingsDto> UpdatePreferencesAsync(long userId, PreferencesSettingsDto dto, CancellationToken ct)
        {
            var s = await GetOrCreateAsync(userId, ct);

            // validations بسيطة
            dto.Language = (dto.Language ?? "ar").Trim().ToLowerInvariant();
            dto.Theme = (dto.Theme ?? "light").Trim().ToLowerInvariant();
            dto.Timezone = (dto.Timezone ?? "Asia/Riyadh").Trim();

            if (dto.Language != "ar" && dto.Language != "en")
                throw new ArgumentException("Language must be ar or en.");

            if (dto.Theme != "light" && dto.Theme != "dark" && dto.Theme != "system")
                throw new ArgumentException("Theme must be light/dark/system.");

            s.Language = dto.Language;
            s.Timezone = dto.Timezone;
            s.Theme = dto.Theme;
            //s.UpdatedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return await GetPreferencesAsync(userId, ct);
        }
    }

}
