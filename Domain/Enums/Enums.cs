using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums
{
    public enum UserType : byte
    {
        Admin = 1,
        Driver = 2,
        Customer = 3
    }

    public enum BookingStatus : byte
    {
        PendingDriverConfirm = 1,
        Confirmed = 2,
        RejectedByDriver = 3,
        CancelledByCustomer = 4,
        CancelledByAdmin = 5,
        Expired = 6,
        Completed = 7

    }
    public enum BookingStatusCustm : byte
    {
        Pending = 1,          // العميل أنشأ الحجز
        Accepted = 2,         // السائق قبل الحجز
        Rejected = 3,         // السائق رفض
        Cancelled = 4,        // العميل ألغى
        Confirmed = 5,        // تأكيد/دفع (سنفعلها بالكامل في مرحلة الدفع)
        Completed = 6         // الرحلة اكتملت
    }


    public enum PaymentMethod : byte
    {
        Card = 1,
        ApplePay = 2,
        Mada = 3,
        Cash = 4
    }

    public enum PaymentStatus : byte
    {
        Pending = 1,
        Paid = 2,
        Failed = 3,
        Refunded = 4
    }

    public enum RefundStatus : byte
    {
        Requested = 1,
        Approved = 2,
        Rejected = 3,
        Processed = 4
    }

    public enum NotificationChannel : byte
    {
        Sms = 1,
        Email = 2,
        Push = 3,
        InApp = 4
    }
    public enum ApprovalStatus : byte
    {
        Approved = 1,
        Pending = 2,
        Rejected = 3
    }

    public enum DriverDocumentType : byte
    {
        NationalId = 1,
        License = 2,
        CarRegistration = 3,
        Other = 4
    }

    public enum DriverDocType { NationalId = 1, License = 2, CarReg = 3 }

}
