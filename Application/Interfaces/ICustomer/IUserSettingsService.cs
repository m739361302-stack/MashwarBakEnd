using Application.DTOs.Auth;
using Application.DTOs.CustomerDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.ICustomer
{
    public interface IUserSettingsService
    {
        Task<NotificationsSettingsDto> GetNotificationsAsync(long userId, CancellationToken ct);
        Task<NotificationsSettingsDto> UpdateNotificationsAsync(long userId, NotificationsSettingsDto dto, CancellationToken ct);

        Task<PreferencesSettingsDto> GetPreferencesAsync(long userId, CancellationToken ct);
        Task<PreferencesSettingsDto> UpdatePreferencesAsync(long userId, PreferencesSettingsDto dto, CancellationToken ct);
    }
}
