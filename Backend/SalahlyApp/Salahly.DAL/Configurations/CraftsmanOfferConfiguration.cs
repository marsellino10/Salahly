using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salahly.DAL.Entities;

namespace Salahly.DAL.Configurations
{
    public class CraftsmanOfferConfiguration : IEntityTypeConfiguration<CraftsmanOffer>
    {
        public void Configure(EntityTypeBuilder<CraftsmanOffer> builder)
        {
            builder.ToTable("CraftsmanOffers");

            // Primary Key
            builder.HasKey(co => co.CraftsmanOfferId);

            // Properties
            builder.Property(co => co.OfferedPrice)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(co => co.Description)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(co => co.EstimatedDurationMinutes)
                .IsRequired();

            builder.Property(co => co.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(co => co.RejectionReason)
                .HasMaxLength(500);

            builder.Property(co => co.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(co => co.ServiceRequest)
                .WithMany(sr => sr.CraftsmanOffers)
                .HasForeignKey(co => co.ServiceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(co => co.Craftsman)
                .WithMany(cr => cr.CraftsmanOffers)
                .HasForeignKey(co => co.CraftsmanId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(co => co.Booking)
                .WithOne(b => b.AcceptedOffer)
                .HasForeignKey<Booking>(b => b.AcceptedOfferId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(co => new { co.ServiceRequestId, co.Status })
                .HasDatabaseName("IX_CraftsmanOffers_Request_Status");

            builder.HasIndex(co => co.CraftsmanId);

            builder.HasIndex(co => new { co.ServiceRequestId, co.CraftsmanId })
                .IsUnique()  
                .HasDatabaseName("IX_CraftsmanOffers_Unique_Request_Craftsman");

            builder.HasIndex(co => co.CreatedAt);
        }
    }
}
