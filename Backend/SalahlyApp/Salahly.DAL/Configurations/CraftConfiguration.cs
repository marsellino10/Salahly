using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salahly.DAL.Entities;

namespace Salahly.DAL.Configurations
{
    public class CraftConfiguration : IEntityTypeConfiguration<Craft>
    {
        public void Configure(EntityTypeBuilder<Craft> builder)
        {
            builder.ToTable("Crafts");

            // Primary Key
            builder.HasKey(c => c.Id);

            // Properties
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.Property(c => c.IconUrl)
                .HasMaxLength(500);

            builder.Property(c => c.IsActive)
                .HasDefaultValue(true);

            builder.Property(c => c.DisplayOrder)
                .HasDefaultValue(0);

            builder.Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships configured in other entities

            // Indexes
            builder.HasIndex(c => c.Name)
                .IsUnique();

            builder.HasIndex(c => new { c.IsActive, c.DisplayOrder });

            
        }
    }
}
