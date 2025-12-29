using Application.DTOs.Auth;
using Application.DTOs.DriverApprovalDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Admin
{
    public interface IAdminDriverApprovalsService
    {
        Task<PagedResultDto<DriverApprovalListItemDto>> GetApprovalsAsync(string status, int? cityId, string? q, int page, int pageSize);
        Task<ApiOkDto> ApproveAsync(long driverUserId, long adminUserId, string? note);
        Task<ApiOkDto> RejectAsync(long driverUserId, long adminUserId, string reason);
    }
}
