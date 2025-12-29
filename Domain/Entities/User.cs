using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
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


        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Approved;
        public DateTime? ApprovalReviewedAt { get; set; }
        public long? ApprovalReviewedByUserId { get; set; }
        public string? ApprovalNote { get; set; }

        // Navigation
        public User? ApprovalReviewedByUser { get; set; }

        public ICollection<Booking> CustomerBookings { get; set; } = new List<Booking>();
        public ICollection<Payment> CustomerPayments { get; set; } = new List<Payment>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<User> ReviewedUsers { get; set; } = new List<User>();
    }
}
