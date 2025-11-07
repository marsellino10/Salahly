using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salahly.DAL.Entities;

namespace Salahly.DAL.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("Reviews");

            // Primary Key
            builder.HasKey(r => r.Id);

            // Properties
            builder.Property(r => r.Rating)
                .IsRequired()
                .HasAnnotation("Range", new[] { 1, 5 }); 

            builder.Property(r => r.Comment)
                .HasMaxLength(1000);

            builder.Property(r => r.CraftsmanResponse)
                .HasMaxLength(1000);

            builder.Property(r => r.IsVerified)
                .HasDefaultValue(false);

            builder.Property(r => r.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(r => r.Booking)
                .WithOne(b => b.Review)
                .HasForeignKey<Review>(r => r.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Customer)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Craftsman)
                .WithMany(cr => cr.Reviews)
                .HasForeignKey(r => r.CraftsmanId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(r => r.BookingId)
                .IsUnique(); 

            builder.HasIndex(r => new { r.CraftsmanId, r.Rating, r.CreatedAt })
                .HasDatabaseName("IX_Reviews_Craftsman_Rating_Date");

            builder.HasIndex(r => r.CustomerId);

            builder.HasIndex(r => new { r.IsVerified, r.CreatedAt });

           
        }
    }
}
