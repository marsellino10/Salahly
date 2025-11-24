using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salahly.DAL.Entities;

namespace Salahly.DAL.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            // Primary Key
            builder.HasKey(p => p.Id);

            // Properties
            builder.Property(p => p.Amount)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(p => p.PaymentDate)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(p => p.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(p => p.TransactionId)
                .HasMaxLength(200);

            builder.Property(p => p.PaymentMethod)
                .HasMaxLength(50);

            builder.Property(p => p.PaymentGateway)
                .HasMaxLength(50);

            builder.Property(p => p.FailureReason)
                .HasMaxLength(500);

            // Relationships
            builder.HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(p => p.BookingId)
                .IsUnique();

            builder.HasIndex(p => p.BookingId)
                .IsUnique();

            builder.HasIndex(p => p.TransactionId)
                .IsUnique()
                .HasFilter("[TransactionId] IS NOT NULL");

            builder.HasIndex(p => new { p.Status, p.PaymentDate })
                .HasDatabaseName("IX_Payments_Status_Date");

            builder.HasIndex(p => p.PaymentMethod);
        }
    }
}
