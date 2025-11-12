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
            builder.HasKey(csa => new { csa.AreaId, csa.CraftsmanId });

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

            // Optional relation to canonical Area table
            builder.HasOne(csa => csa.Area)
                .WithMany(a => a.CraftsmanServiceAreas)
                .HasForeignKey(csa => csa.AreaId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes: index on CraftsmanId only; location indexes moved to Areas table
            builder.HasIndex(csa => csa.CraftsmanId);
        }
    }
}
