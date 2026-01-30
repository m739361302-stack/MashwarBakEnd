using Application.DTOs.Admin;
using Application.DTOs.Auth;
using Application.DTOs.DriverApprovalDtos;
using Application.Interfaces.Admin;
using Application.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Application.DTOs.Admin.AdminDriverDtos;

namespace APIs.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("api/[controller]")]
    //[ApiController]
    //[Authorize(Policy = Policies.AdminOnly)]
    public class AdminDriversController : ControllerBase
    {
        private readonly IAdminDriverApprovalsService _svc;

        public AdminDriversController(IAdminDriverApprovalsService svc)
        {
            _svc = svc;
        }

        [HttpGet("approvals")]
        public async Task<ActionResult<PagedResultDto<DriverApprovalListItemDto>>> GetApprovals(
            [FromQuery] string status = "pending",
            [FromQuery] int? cityId = null,
            [FromQuery] string? q = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20
        )
        {
            var res = await _svc.GetApprovalsAsync(status, cityId, q, page, pageSize);
            return Ok(res);
        }
  
        [HttpPost("driver-approvals/{driverUserId:long}/approve")]
        public async Task<ActionResult<ApiOkDto>> Approve(long driverUserId, [FromBody] ApproveDriverRequestDto dto)
        {
            var claims = User?.Claims?.Select(c => new { c.Type, c.Value }).ToList();

            var adminIdStr = User.FindFirstValue("uid");
            if (!long.TryParse(adminIdStr, out var adminId))
                return Unauthorized(new { message = "Invalid token" });

            try
            {
                var res = await _svc.ApproveAsync(driverUserId, adminId, dto.Note);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("driver-approvals/{driverUserId:long}/reject")]
        public async Task<ActionResult<ApiOkDto>> Reject(long driverUserId, [FromBody] RejectDriverRequestDto dto)
        {
            var adminIdStr = User.FindFirstValue("uid");
            if (!long.TryParse(adminIdStr, out var adminId))
                return Unauthorized(new { message = "Invalid token" });

            try
            {
                var res = await _svc.RejectAsync(driverUserId, adminId, dto.Reason);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("dashboard/summary")]
        public async Task<ActionResult<AdminDashboardSummaryDto>> GetDashboardSummary(CancellationToken ct)
        {
            return Ok(await _svc.GetDashboardSummaryAsync(ct));
        }

   
        [HttpGet("getAdminDrivers")]
        public async Task<ActionResult<List<AdminDriverListItemDto>>> Get(CancellationToken ct)
        {
            var list = await _svc.GetAdminDriversAsync(ct);
            return Ok(list);
        }

        [HttpGet("getDriverApprovals")]
        public async Task<ActionResult<List<AdminDriverListItemDto>>> GetDriverApprovals(CancellationToken ct)
        {
            var list = await _svc.GetDriverApprovalsAsync(ct);
            return Ok(list);
        }


        [HttpGet("getDriverApprovalDetails/{driverUserId:long}")]
        public async Task<ActionResult<List<AdminDriverListItemDto>>> GetDriverApprovalDetails(long driverUserId,CancellationToken ct)
        {
            //var userId = User.GetUserId();
            var list = await _svc.GetDriverApprovalDetailsAsync(driverUserId, ct);
            return Ok(list);
        }


        //[HttpGet("{userId:long}")]
        //public async Task<ActionResult<AdminDriverListItemDto>> GetById(long userId, CancellationToken ct)
        //{
        //    var item = await _admin.GetAdminDriverDetailsAsync(userId, ct); // اختياري
        //    if (item == null) return NotFound(new { message = "Driver not found" });
        //    return Ok(item);
        //}

        //[HttpPut("{userId:long}/active")]
        //public async Task<ActionResult> SetActive(long userId, [FromBody] AdminToggleActiveRequest req, CancellationToken ct)
        //{
        //    await _admin.SetDriverActiveAsync(userId, req.IsActive, ct);
        //    return Ok(new { message = "Updated" });
        //}

        //// ✅ اختياري فقط لو تريد الأدمن يتحكم بتوفر السائق
        //[HttpPut("{userId:long}/availability")]
        //public async Task<ActionResult> SetAvailability(long userId, [FromBody] AdminToggleAvailabilityRequest req, CancellationToken ct)
        //{
        //    await _admin.SetDriverAvailabilityAsync(userId, req.IsAvailableNow, ct);
        //    return Ok(new { message = "Updated" });
        //}


    }
}
