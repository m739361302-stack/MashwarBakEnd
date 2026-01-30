using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Admin
{
    public sealed record AdminReportsFilterDto(
     string Range,        // today | last7 | last30 | custom
     DateOnly? FromDate,
     DateOnly? ToDate,
     string Status,       // all | PendingDriverConfirm | Confirmed | ...
     string FromCity,     // all | cityId or name (الأفضل cityId)
     string ToCity        // all | cityId or name
 );
}
