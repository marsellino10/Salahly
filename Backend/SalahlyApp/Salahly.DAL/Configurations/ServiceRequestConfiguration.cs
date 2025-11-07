using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salahly.DAL.Entities;

namespace Salahly.DAL.Configurations
{
    public class ServiceRequestConfiguration : IEntityTypeConfiguration<ServiceRequest>
    {
        public void Configure(EntityTypeBuilder<ServiceRequest> builder)
        {
            builder.ToTable("ServiceRequests");

            // Primary Key
            builder.HasKey(sr => sr.ServiceRequestId);

            // Properties
            builder.Property(sr => sr.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(sr => sr.Description)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(sr => sr.Address)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(sr => sr.City)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sr => sr.Area)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sr => sr.Latitude)
                .HasPrecision(10, 7);

            builder.Property(sr => sr.Longitude)
                .HasPrecision(10, 7);

            builder.Property(sr => sr.PreferredTimeSlot)
                .HasMaxLength(50);

            builder.Property(sr => sr.CustomerBudget)
                .HasPrecision(10, 2);

            builder.Property(sr => sr.ImagesJson)
                .HasColumnType("nvarchar(max)");

            builder.Property(sr => sr.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(sr => sr.OffersCount)
                .HasDefaultValue(0);

            builder.Property(sr => sr.MaxOffers)
                .HasDefaultValue(10);

            builder.Property(sr => sr.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(sr => sr.Customer)
                .WithMany(c => c.ServiceRequests)
                .HasForeignKey(sr => sr.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sr => sr.Craft)
                .WithMany(c => c.ServiceRequests)
                .HasForeignKey(sr => sr.CraftId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(sr => sr.CraftsmanOffers)
                .WithOne(co => co.ServiceRequest)
                .HasForeignKey(co => co.ServiceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sr => sr.Booking)
                .WithOne(b => b.ServiceRequest)
                .HasForeignKey<Booking>(b => b.ServiceRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(sr => new { sr.City, sr.Area, sr.Status })
                .HasDatabaseName("IX_ServiceRequests_Location_Status");

            builder.HasIndex(sr => new { sr.CraftId, sr.Status, sr.CreatedAt })
                .HasDatabaseName("IX_ServiceRequests_Craft_Status_Date");

            builder.HasIndex(sr => sr.CustomerId);

            builder.HasIndex(sr => sr.ExpiresAt)
                .HasFilter("[Status] IN (0, 1)") 
                .HasDatabaseName("IX_ServiceRequests_Expiration");
        }
    }
}
