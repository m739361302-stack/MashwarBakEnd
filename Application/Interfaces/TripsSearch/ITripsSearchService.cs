using Application.DTOs.Result;
using Application.DTOs.SearchTripsDto;
using Application.DTOs.Trips;
using System;
using System.Collections.Generic;
using System.Text;
using static Application.DTOs.Trips.AdminTripDtos;

namespace Application.Interfaces.TripsSearch
{
    public interface ITripsSearchService
    {
        Task<ApiResult<List<TripSearchItemDto>>> SearchAsync(
            long? userId,
            SearchTripsQueryDto q,
            bool requireAvailabilitySlot,
            CancellationToken ct);
        Task<List<TopTripDto>> GetTopTripsAsync(
        int? fromCityId,
        int? toCityId,
        DateTime? date,
        int limit,
        CancellationToken ct);
        Task<List<AdminTripListItemDto>> GetTripsAsync(CancellationToken ct);
        Task<AdminTripDetailsDto?> GetTripDetailsAsync(long tripId, CancellationToken ct);
        Task<bool> SetTripActiveAsync(long tripId, bool isActive, CancellationToken ct);
    }

}
