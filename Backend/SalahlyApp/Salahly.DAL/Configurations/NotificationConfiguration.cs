using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salahly.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            // Primary Key
            builder.HasKey(n => n.NotificationId);

            // Properties
            builder.Property(n => n.Type)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(n => n.ActionUrl)
                .HasMaxLength(500);

            builder.Property(n => n.IsRead)
                .HasDefaultValue(false);

            builder.Property(n => n.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(n => n.ServiceRequest)
                .WithMany()  
                .HasForeignKey(n => n.ServiceRequestId)
                .OnDelete(DeleteBehavior.SetNull); 

            builder.HasOne(n => n.CraftsmanOffer)
                .WithMany()
                .HasForeignKey(n => n.CraftsmanOfferId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(n => n.Booking)
                .WithMany()
                .HasForeignKey(n => n.BookingId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt })
                .HasDatabaseName("IX_Notifications_User_Read_Date");

            builder.HasIndex(n => n.Type);

            builder.HasIndex(n => n.ServiceRequestId);

            builder.HasIndex(n => n.CraftsmanOfferId);

            builder.HasIndex(n => n.BookingId);

            // Composite index for unread notifications
            builder.HasIndex(n => new { n.UserId, n.IsRead })
                .HasFilter("[IsRead] = 0")
                .HasDatabaseName("IX_Notifications_Unread");
        }
    }
}
