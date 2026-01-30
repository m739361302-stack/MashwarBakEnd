using Application.DTOs.Admin;
using Application.DTOs.Auth;
using Application.DTOs.DriverApprovalDtos;
using System;
using System.Collections.Generic;
using System.Text;
using static Application.DTOs.Admin.AdminDriverDtos;
using static Application.DTOs.DriverDto.AdminDriverDtos;

namespace Application.Interfaces.Admin
{
    public interface IAdminDriverApprovalsService
    {
        Task<PagedResultDto<DriverApprovalListItemDto>> GetApprovalsAsync(string status, int? cityId, string? q, int page, int pageSize);
        Task<ApiOkDto> ApproveAsync(long driverUserId, long adminUserId, string? note);
        Task<ApiOkDto> RejectAsync(long driverUserId, long adminUserId, string reason);

        Task<AdminDashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken ct);
        Task<List<AdminDriverListItemDto>> GetAdminDriversAsync(CancellationToken ct);
        Task<List<AdminDriverApprovalListItemDto>> GetDriverApprovalsAsync(CancellationToken ct);
        Task<AdminDriverApprovalDetailsDto?> GetDriverApprovalDetailsAsync(long driverUserId, CancellationToken ct);
    }
}
