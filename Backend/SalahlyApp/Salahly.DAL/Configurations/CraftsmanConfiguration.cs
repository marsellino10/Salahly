using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salahly.DAL.Entities;

namespace Salahly.DAL.Configurations
{
    public class CraftsmanConfiguration : IEntityTypeConfiguration<Craftsman>
    {
        public void Configure(EntityTypeBuilder<Craftsman> builder)
        {
            builder.ToTable("Craftsmen");

            // Primary Key
            builder.HasKey(cr => cr.Id);

            builder.Property(cr => cr.TotalCompletedBookings)
                .HasDefaultValue(0);

            builder.Property(cr => cr.IsAvailable)
                .HasDefaultValue(true);

            builder.Property(cr => cr.HourlyRate)
                .HasPrecision(10, 2);

            builder.Property(cr => cr.Bio)
                .HasMaxLength(2000);

            builder.Property(cr => cr.YearsOfExperience)
                .HasDefaultValue(0);

            // Relationships
            builder.HasOne(cr => cr.Craft)
                .WithMany(c => c.Craftsmen)
                .HasForeignKey(cr => cr.CraftId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(cr => cr.CraftsmanServiceAreas)
                .WithOne(csa => csa.Craftsman)
                .HasForeignKey(csa => csa.CraftsmanId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(cr => cr.CraftsmanOffers)
                .WithOne(co => co.Craftsman)
                .HasForeignKey(co => co.CraftsmanId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(cr => cr.Portfolio)
                .WithOne(p => p.Craftsman)
                .HasForeignKey(p => p.CraftsmanId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(cr => cr.Bookings)
                .WithOne(b => b.Craftsman)
                .HasForeignKey(b => b.CraftsmanId)
                .OnDelete(DeleteBehavior.Restrict);

            //builder.HasMany(cr => cr.Reviews)
            //    .WithOne(r => r.Craftsman)
            //    .HasForeignKey(r => r.CraftsmanId)
            //    .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(cr => cr.CraftId);

            //builder.HasIndex(cr => new { cr.User.RatingAverage, cr.IsAvailable });

            //builder.HasIndex(cr => new { cr.CraftId, cr.IsAvailable, cr.User.RatingAverage })
            //    .HasDatabaseName("IX_Craftsmen_Craft_Available_Rating");
        }
    }
}
