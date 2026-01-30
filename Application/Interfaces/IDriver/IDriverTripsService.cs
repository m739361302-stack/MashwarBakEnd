using Application.DTOs.Result;
using System;
using System.Collections.Generic;
using System.Text;
using static Application.DTOs.Trips.TripsDto;

namespace Application.Interfaces.IDriver
{
    public interface IDriverTripsService
    {
        Task<ApiResult<List<DriverTripListItemDto>>> GetMyTripsAsync(
            long userId,
            string? status,
            int? from,
            int? to,
            DateTime? dateFrom,
            CancellationToken ct);

        Task<ApiResult<long>> CreateTripAsync(long userId, CreateTripDto dto, CancellationToken ct);
        Task<ApiResult> UpdateTripAsync(long userId, int tripId, UpdateTripDto dto, CancellationToken ct);

        Task<ApiResult> ActivateTripAsync(long userId, int tripId, CancellationToken ct);
        Task<ApiResult> DeactivateTripAsync(long userId, int tripId, CancellationToken ct);
    }

}
