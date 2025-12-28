using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Entities
    {
        public class User
        {
            public long Id { get; set; }
            public UserType UserType { get; set; }
            public string FullName { get; set; } = null!;
            public string Phone { get; set; } = null!;
            public string? Email { get; set; }
            public string PasswordHash { get; set; } = null!;
            public bool IsActive { get; set; } = true;
            public DateTime? LastLoginAt { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }

            public Customer? Customer { get; set; }
            public Driver? Driver { get; set; }
            public UserSettings? UserSettings { get; set; }

            public ICollection<Booking> CustomerBookings { get; set; } = new List<Booking>();
            public ICollection<Payment> CustomerPayments { get; set; } = new List<Payment>();
            public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        }

        public class Customer
        {
            public long UserId { get; set; }
            public int? CityId { get; set; }
            public string? AvatarUrl { get; set; }

            public User User { get; set; } = null!;
            public City? City { get; set; }
        }

        public class Driver
        {
            public long UserId { get; set; }
            public int? CityId { get; set; }
            public bool IsAvailable { get; set; }
            public decimal RatingAvg { get; set; }
            public int RatingCount { get; set; }

            public User User { get; set; } = null!;
            public City? City { get; set; }

            public ICollection<DriverVehicle> Vehicles { get; set; } = new List<DriverVehicle>();
            public ICollection<DriverAvailabilitySlot> AvailabilitySlots { get; set; } = new List<DriverAvailabilitySlot>();
            public ICollection<Trip> Trips { get; set; } = new List<Trip>();
            public ICollection<Booking> DriverBookings { get; set; } = new List<Booking>();
        }

        public class DriverVehicle
        {
            public long Id { get; set; }
            public long DriverUserId { get; set; }
            public string? Make { get; set; }
            public string? Model { get; set; }
            public short? Year { get; set; }
            public string? PlateNumber { get; set; }
            public string? Color { get; set; }
            public bool IsPrimary { get; set; }
            public DateTime CreatedAt { get; set; }

            public Driver Driver { get; set; } = null!;
        }

        public class DriverAvailabilitySlot
        {
            public long Id { get; set; }
            public long DriverUserId { get; set; }
            public DateTime StartAt { get; set; }
            public DateTime EndAt { get; set; }
            public bool IsActive { get; set; }
            public string? Notes { get; set; }
            public DateTime CreatedAt { get; set; }

            public Driver Driver { get; set; } = null!;
        }

        public class City
        {
            public int Id { get; set; }
            public string NameAr { get; set; } = null!;
            public string? NameEn { get; set; }
            public bool IsActive { get; set; }
            public int SortOrder { get; set; }

            public ICollection<CityRoute> FromRoutes { get; set; } = new List<CityRoute>();
            public ICollection<CityRoute> ToRoutes { get; set; } = new List<CityRoute>();
        }

        public class CityRoute
        {
            public long Id { get; set; }
            public int FromCityId { get; set; }
            public int ToCityId { get; set; }
            public bool IsActive { get; set; }

            public City FromCity { get; set; } = null!;
            public City ToCity { get; set; } = null!;
        }

        public class Trip
        {
            public long Id { get; set; }
            public long DriverUserId { get; set; }
            public int FromCityId { get; set; }
            public int ToCityId { get; set; }
            public DateTime DepartAt { get; set; }
            public decimal Price { get; set; }
            public byte Seats { get; set; } = 1;
            public string? Notes { get; set; }
            public bool IsActive { get; set; } = true;
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }

            public Driver Driver { get; set; } = null!;
            public City FromCity { get; set; } = null!;
            public City ToCity { get; set; } = null!;

            public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        }

        public class Booking
        {
            public long Id { get; set; }
            public long TripId { get; set; }
            public long CustomerUserId { get; set; }
            public long DriverUserId { get; set; }
            public BookingStatus BookingStatus { get; set; }
            public decimal PriceSnapshot { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? ConfirmedAt { get; set; }
            public DateTime? CancelledAt { get; set; }
            public string? CancelReason { get; set; }

            public Trip Trip { get; set; } = null!;
            public User Customer { get; set; } = null!;
            public Driver Driver { get; set; } = null!;

            public ICollection<BookingStatusHistory> StatusHistory { get; set; } = new List<BookingStatusHistory>();
            public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        }

        public class BookingStatusHistory
        {
            public long Id { get; set; }
            public long BookingId { get; set; }
            public BookingStatus OldStatus { get; set; }
            public BookingStatus NewStatus { get; set; }
            public long? ChangedByUserId { get; set; }
            public DateTime ChangedAt { get; set; }
            public string? Notes { get; set; }

            public Booking Booking { get; set; } = null!;
            public User? ChangedByUser { get; set; }
        }

        public class Payment
        {
            public long Id { get; set; }
            public string PaymentCode { get; set; } = null!;
            public long BookingId { get; set; }
            public long CustomerUserId { get; set; }
            public decimal Amount { get; set; }
            public string Currency { get; set; } = "SAR";
            public PaymentMethod Method { get; set; }
            public PaymentStatus Status { get; set; }
            public string? Provider { get; set; }
            public string? ProviderReference { get; set; }
            public DateTime? PaidAt { get; set; }
            public DateTime CreatedAt { get; set; }
            public string? Note { get; set; }

            public Booking Booking { get; set; } = null!;
            public User Customer { get; set; } = null!;
            public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
        }

        public class Refund
        {
            public long Id { get; set; }
            public long PaymentId { get; set; }
            public decimal Amount { get; set; }
            public RefundStatus Status { get; set; }
            public DateTime RequestedAt { get; set; }
            public DateTime? ProcessedAt { get; set; }
            public string? Reason { get; set; }

            public Payment Payment { get; set; } = null!;
        }

        public class AppSetting
        {
            public int Id { get; set; }
            public string Key { get; set; } = null!;
            public string Value { get; set; } = null!;
            public string? ValueType { get; set; }
            public DateTime UpdatedAt { get; set; }
            public long? UpdatedByUserId { get; set; }

            public User? UpdatedByUser { get; set; }
        }

        public class UserSettings
        {
            public long UserId { get; set; }
            public string Language { get; set; } = "ar";
            public string Theme { get; set; } = "light";
            public string Timezone { get; set; } = "Asia/Riyadh";
            public bool NotifyBookingUpdates { get; set; } = true;
            public bool NotifyTripReminders { get; set; } = true;
            public bool NotifyPromotions { get; set; } = false;
            public bool NotifySms { get; set; } = true;
            public bool NotifyEmail { get; set; } = false;
            public DateTime UpdatedAt { get; set; }

            public User User { get; set; } = null!;
        }

        public class Notification
        {
            public long Id { get; set; }
            public long UserId { get; set; }
            public NotificationChannel Channel { get; set; }
            public string? Title { get; set; }
            public string Body { get; set; } = null!;
            public string? RelatedType { get; set; }
            public long? RelatedId { get; set; }
            public bool IsRead { get; set; }
            public DateTime? SentAt { get; set; }
            public DateTime CreatedAt { get; set; }

            public User User { get; set; } = null!;
        }

    }
}
