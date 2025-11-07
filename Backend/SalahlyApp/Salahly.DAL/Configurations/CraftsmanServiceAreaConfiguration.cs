using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salahly.DAL.Entities;

namespace Salahly.DAL.Configurations
{
    public class CraftsmanServiceAreaConfiguration : IEntityTypeConfiguration<CraftsmanServiceArea>
    {
        public void Configure(EntityTypeBuilder<CraftsmanServiceArea> builder)
        {
            builder.ToTable("CraftsmanServiceAreas");

            // Primary Key
            builder.HasKey(csa => csa.CraftsmanServiceAreaId);

            // Properties
            builder.Property(csa => csa.City)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(csa => csa.Area)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(csa => csa.ServiceRadiusKm)
                .IsRequired()
                .HasDefaultValue(10);

            builder.Property(csa => csa.IsActive)
                .HasDefaultValue(true);

            builder.Property(csa => csa.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(csa => csa.Craftsman)
                .WithMany(cr => cr.CraftsmanServiceAreas)
                .HasForeignKey(csa => csa.CraftsmanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(csa => new { csa.City, csa.Area, csa.IsActive })
                .HasDatabaseName("IX_CraftsmanServiceAreas_Location_Active");

            builder.HasIndex(csa => csa.CraftsmanId);

            builder.HasIndex(csa => new { csa.CraftsmanId, csa.City, csa.Area })
                .IsUnique() 
                .HasDatabaseName("IX_CraftsmanServiceAreas_Unique");
        }
    }
}
