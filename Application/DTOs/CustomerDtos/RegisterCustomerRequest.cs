using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.CustomerDtos
{

    public sealed class RegisterCustomerRequest
    {
        public string Phone { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public sealed class RegisterCustomerResponse
    {
        public long UserId { get; set; }
        public string Message { get; set; } = "تم تسجيل العميل بنجاح";
    }
}
