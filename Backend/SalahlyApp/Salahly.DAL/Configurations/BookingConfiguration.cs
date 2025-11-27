using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salahly.DAL.Entities;

namespace Salahly.DAL.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("Bookings");

            // Primary Key
            builder.HasKey(b => b.BookingId);

            // Properties
            builder.Property(b => b.Duration)
                .IsRequired();

            builder.Property(b => b.TotalAmount)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(b => b.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(b => b.Notes)
                .HasMaxLength(1000);

            builder.Property(b => b.CancellationReason)
                .HasMaxLength(500);

            builder.Property(b => b.CompletionNotes)
                .HasMaxLength(1000);

            builder.Property(b => b.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(b => b.Customer)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Craftsman)
                .WithMany(cr => cr.Bookings)
                .HasForeignKey(b => b.CraftsmanId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Craft)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CraftId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.ServiceRequest)
                .WithOne(sr => sr.Booking)
                .HasForeignKey<Booking>(b => b.ServiceRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.AcceptedOffer)
                .WithOne(co => co.Booking)
                .HasForeignKey<Booking>(b => b.AcceptedOfferId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.HasMany(b => b.Reviews)
                .WithOne(r => r.Booking)
                .HasForeignKey(r => r.BookingId)
                .OnDelete(DeleteBehavior.Cascade);


            // Indexes
            builder.HasIndex(b => new { b.CustomerId, b.Status })
                .HasDatabaseName("IX_Bookings_Customer_Status");

            builder.HasIndex(b => new { b.CraftsmanId, b.Status, b.BookingDate })
                .HasDatabaseName("IX_Bookings_Craftsman_Status_Date");

            builder.HasIndex(b => b.BookingDate);

            builder.HasIndex(b => b.ServiceRequestId)
                .IsUnique()
                .HasFilter("[ServiceRequestId] IS NOT NULL");
        }
    }
}
