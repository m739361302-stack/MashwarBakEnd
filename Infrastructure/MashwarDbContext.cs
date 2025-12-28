<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure
{
    using Microsoft.EntityFrameworkCore;

=======
﻿using Microsoft.EntityFrameworkCore;


using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using static Domain.Entities.Mashwar;

namespace Infrastructure
{
>>>>>>> fdfa5487b0aa738d95ef40ec1f6239967af2d1ca
    public class MashwarDbContext : DbContext
    {
        public MashwarDbContext(DbContextOptions<MashwarDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Driver> Drivers => Set<Driver>();
        public DbSet<DriverVehicle> DriverVehicles => Set<DriverVehicle>();
        public DbSet<DriverAvailabilitySlot> DriverAvailabilitySlots => Set<DriverAvailabilitySlot>();
        public DbSet<City> Cities => Set<City>();
        public DbSet<CityRoute> CityRoutes => Set<CityRoute>();
        public DbSet<Trip> Trips => Set<Trip>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<BookingStatusHistory> BookingStatusHistories => Set<BookingStatusHistory>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Refund> Refunds => Set<Refund>();
        public DbSet<AppSetting> AppSettings => Set<AppSetting>();
        public DbSet<UserSettings> UserSettings => Set<UserSettings>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("Users");
                e.HasKey(x => x.Id);

                e.Property(x => x.UserType).HasConversion<byte>();
                e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
                e.Property(x => x.Phone).HasMaxLength(20).IsRequired();
                e.Property(x => x.Email).HasMaxLength(150);
                e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();

                e.HasIndex(x => x.Phone).IsUnique();
                e.HasIndex(x => x.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
                e.HasIndex(x => x.UserType);
                e.HasIndex(x => x.IsActive);

                e.HasOne(x => x.Customer).WithOne(x => x.User).HasForeignKey<Customer>(x => x.UserId);
                e.HasOne(x => x.Driver).WithOne(x => x.User).HasForeignKey<Driver>(x => x.UserId);
                e.HasOne(x => x.UserSettings).WithOne(x => x.User).HasForeignKey<UserSettings>(x => x.UserId);
<<<<<<< HEAD
=======

                // NEW: self reference for reviewed by
                e.HasOne(x => x.ApprovalReviewedByUser)
                    .WithMany() // أو .WithMany(u => u.ReviewedUsers) لو أضفتها
                    .HasForeignKey(x => x.ApprovalReviewedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);

>>>>>>> fdfa5487b0aa738d95ef40ec1f6239967af2d1ca
            });

            modelBuilder.Entity<City>(e =>
            {
                e.ToTable("Cities");
                e.HasKey(x => x.Id);
                e.Property(x => x.NameAr).HasMaxLength(100).IsRequired();
                e.Property(x => x.NameEn).HasMaxLength(100);
                e.HasIndex(x => x.NameAr).IsUnique();
                e.HasIndex(x => x.IsActive);
            });

            modelBuilder.Entity<CityRoute>(e =>
            {
                e.ToTable("CityRoutes");
                e.HasKey(x => x.Id);
                e.HasIndex(x => new { x.FromCityId, x.ToCityId }).IsUnique();

                e.HasOne(x => x.FromCity).WithMany(x => x.FromRoutes).HasForeignKey(x => x.FromCityId).OnDelete(DeleteBehavior.NoAction);
                e.HasOne(x => x.ToCity).WithMany(x => x.ToRoutes).HasForeignKey(x => x.ToCityId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Customer>(e =>
            {
                e.ToTable("Customers");
                e.HasKey(x => x.UserId);
                e.Property(x => x.AvatarUrl).HasMaxLength(500);
                e.HasIndex(x => x.CityId);

                e.HasOne(x => x.City).WithMany().HasForeignKey(x => x.CityId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Driver>(e =>
            {
                e.ToTable("Drivers");
                e.HasKey(x => x.UserId);

                e.Property(x => x.RatingAvg).HasPrecision(3, 2);
                e.HasIndex(x => x.CityId);
                e.HasIndex(x => x.IsAvailable);

                e.HasOne(x => x.City).WithMany().HasForeignKey(x => x.CityId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<DriverVehicle>(e =>
            {
                e.ToTable("DriverVehicles");
                e.HasKey(x => x.Id);

                e.Property(x => x.Make).HasMaxLength(50);
                e.Property(x => x.Model).HasMaxLength(50);
                e.Property(x => x.PlateNumber).HasMaxLength(20);
                e.Property(x => x.Color).HasMaxLength(30);

                e.HasIndex(x => x.DriverUserId);

                e.HasOne(x => x.Driver).WithMany(x => x.Vehicles).HasForeignKey(x => x.DriverUserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DriverAvailabilitySlot>(e =>
            {
                e.ToTable("DriverAvailabilitySlots");
                e.HasKey(x => x.Id);

                e.Property(x => x.Notes).HasMaxLength(250);

                e.HasIndex(x => new { x.DriverUserId, x.StartAt });
                e.HasIndex(x => new { x.DriverUserId, x.EndAt });
                e.HasIndex(x => x.IsActive);

                e.HasOne(x => x.Driver).WithMany(x => x.AvailabilitySlots).HasForeignKey(x => x.DriverUserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Trip>(e =>
            {
                e.ToTable("Trips");
                e.HasKey(x => x.Id);

                e.Property(x => x.Price).HasPrecision(10, 2);
                e.Property(x => x.Seats).HasDefaultValue((byte)1);

                e.HasIndex(x => new { x.FromCityId, x.ToCityId, x.DepartAt });
                e.HasIndex(x => new { x.DriverUserId, x.DepartAt });
                e.HasIndex(x => x.IsActive);

                e.HasOne(x => x.Driver).WithMany(x => x.Trips).HasForeignKey(x => x.DriverUserId).OnDelete(DeleteBehavior.NoAction);
                e.HasOne(x => x.FromCity).WithMany().HasForeignKey(x => x.FromCityId).OnDelete(DeleteBehavior.NoAction);
                e.HasOne(x => x.ToCity).WithMany().HasForeignKey(x => x.ToCityId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Booking>(e =>
            {
                e.ToTable("Bookings");
                e.HasKey(x => x.Id);

                e.Property(x => x.BookingStatus).HasConversion<byte>();
                e.Property(x => x.PriceSnapshot).HasPrecision(10, 2);

                e.HasIndex(x => x.TripId);
                e.HasIndex(x => x.BookingStatus);
                e.HasIndex(x => new { x.CustomerUserId, x.CreatedAt });
                e.HasIndex(x => new { x.DriverUserId, x.CreatedAt });

                e.HasOne(x => x.Trip).WithMany(x => x.Bookings).HasForeignKey(x => x.TripId).OnDelete(DeleteBehavior.NoAction);
                e.HasOne(x => x.Customer).WithMany(x => x.CustomerBookings).HasForeignKey(x => x.CustomerUserId).OnDelete(DeleteBehavior.NoAction);
                e.HasOne(x => x.Driver).WithMany(x => x.DriverBookings).HasForeignKey(x => x.DriverUserId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<BookingStatusHistory>(e =>
            {
                e.ToTable("BookingStatusHistory");
                e.HasKey(x => x.Id);

                e.Property(x => x.OldStatus).HasConversion<byte>();
                e.Property(x => x.NewStatus).HasConversion<byte>();
                e.Property(x => x.Notes).HasMaxLength(250);

                e.HasIndex(x => new { x.BookingId, x.ChangedAt });

                e.HasOne(x => x.Booking).WithMany(x => x.StatusHistory).HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.ChangedByUser).WithMany().HasForeignKey(x => x.ChangedByUserId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Payment>(e =>
            {
                e.ToTable("Payments");
                e.HasKey(x => x.Id);

                e.Property(x => x.PaymentCode).HasMaxLength(40).IsRequired();
                e.Property(x => x.Currency).HasMaxLength(10).IsRequired();
                e.Property(x => x.Amount).HasPrecision(10, 2);
                e.Property(x => x.Method).HasConversion<byte>();
                e.Property(x => x.Status).HasConversion<byte>();
                e.Property(x => x.Provider).HasMaxLength(50);
                e.Property(x => x.ProviderReference).HasMaxLength(100);
                e.Property(x => x.Note).HasMaxLength(250);

                e.HasIndex(x => x.PaymentCode).IsUnique();
                e.HasIndex(x => x.BookingId);
                e.HasIndex(x => new { x.CustomerUserId, x.CreatedAt });
                e.HasIndex(x => x.Status);

                e.HasOne(x => x.Booking).WithMany(x => x.Payments).HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.NoAction);
                e.HasOne(x => x.Customer).WithMany(x => x.CustomerPayments).HasForeignKey(x => x.CustomerUserId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Refund>(e =>
            {
                e.ToTable("Refunds");
                e.HasKey(x => x.Id);

                e.Property(x => x.Amount).HasPrecision(10, 2);
                e.Property(x => x.Status).HasConversion<byte>();
                e.Property(x => x.Reason).HasMaxLength(250);

                e.HasIndex(x => x.PaymentId);
                e.HasIndex(x => x.Status);

                e.HasOne(x => x.Payment).WithMany(x => x.Refunds).HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AppSetting>(e =>
            {
                e.ToTable("AppSettings");
                e.HasKey(x => x.Id);

                e.Property(x => x.Key).HasMaxLength(100).IsRequired();
                e.Property(x => x.Value).IsRequired();
                e.Property(x => x.ValueType).HasMaxLength(20);

                e.HasIndex(x => x.Key).IsUnique();

                e.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<UserSettings>(e =>
            {
                e.ToTable("UserSettings");
                e.HasKey(x => x.UserId);

                e.Property(x => x.Language).HasMaxLength(10).IsRequired();
                e.Property(x => x.Theme).HasMaxLength(10).IsRequired();
                e.Property(x => x.Timezone).HasMaxLength(50).IsRequired();
            });

            modelBuilder.Entity<Notification>(e =>
            {
                e.ToTable("Notifications");
                e.HasKey(x => x.Id);

                e.Property(x => x.Channel).HasConversion<byte>();
                e.Property(x => x.Title).HasMaxLength(120);
                e.Property(x => x.Body).HasMaxLength(500).IsRequired();
                e.Property(x => x.RelatedType).HasMaxLength(50);

                e.HasIndex(x => new { x.UserId, x.CreatedAt });
                e.HasIndex(x => x.IsRead);

                e.HasOne(x => x.User).WithMany(x => x.Notifications).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            });
<<<<<<< HEAD
        }
    }

=======
            modelBuilder.Entity<Driver>(e =>
            {
                e.ToTable("Drivers");
                e.HasKey(x => x.UserId);

                e.Property(x => x.RatingAvg).HasPrecision(3, 2);

                // NEW fields
                e.Property(x => x.NationalId).HasMaxLength(20);
                e.Property(x => x.LicenseNumber).HasMaxLength(50);
                e.Property(x => x.Iban).HasMaxLength(34);

                // إذا استخدمت DateOnly:
                // e.Property(x => x.LicenseExpiry).HasColumnType("date");

                e.HasIndex(x => x.CityId);
                e.HasIndex(x => x.IsAvailable);

                // Unique filtered indexes (مثل SQL)
                e.HasIndex(x => x.NationalId).IsUnique().HasFilter("[NationalId] IS NOT NULL");
                e.HasIndex(x => x.LicenseNumber).IsUnique().HasFilter("[LicenseNumber] IS NOT NULL");

                e.HasOne(x => x.City).WithMany().HasForeignKey(x => x.CityId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<DriverDocument>(e =>
            {
                e.ToTable("DriverDocuments");
                e.HasKey(x => x.Id);

                e.Property(x => x.DocType).HasConversion<byte>();
                e.Property(x => x.FileUrl).HasMaxLength(500).IsRequired();
                e.Property(x => x.Note).HasMaxLength(250);

                e.HasIndex(x => new { x.DriverUserId, x.UploadedAt });
                e.HasIndex(x => x.DocType);

                e.HasOne(x => x.Driver)
                    .WithMany(d => d.Documents)
                    .HasForeignKey(x => x.DriverUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


        }
    }
>>>>>>> fdfa5487b0aa738d95ef40ec1f6239967af2d1ca
}
