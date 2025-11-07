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

            // Seed Data
            builder.HasData(
                new Craft { Id = 1, Name = "Electrician", Description = "Electrical repairs and installations", DisplayOrder = 1, IsActive = true },
                new Craft { Id = 2, Name = "Plumber", Description = "Plumbing services", DisplayOrder = 2, IsActive = true },
                new Craft { Id = 3, Name = "Carpenter", Description = "Carpentry and woodwork", DisplayOrder = 3, IsActive = true },
                new Craft { Id = 4, Name = "Painter", Description = "Painting services", DisplayOrder = 4, IsActive = true },
                new Craft { Id = 5, Name = "AC Technician", Description = "Air conditioning repair and maintenance", DisplayOrder = 5, IsActive = true }
            );
        }
    }
}
