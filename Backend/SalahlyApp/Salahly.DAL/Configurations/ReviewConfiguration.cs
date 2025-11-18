using System.Reflection.Emit;
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


            builder.Property(r => r.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            //builder.HasOne(r => r.Booking)
            //    .WithOne(b => b.Review)
            //    .HasForeignKey<Review>(r => r.BookingId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //builder.HasOne(r => r.Customer)
            //    .WithMany(c => c.Reviews)
            //    .HasForeignKey(r => r.CustomerId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //builder.HasOne(r => r.Craftsman)
            //    .WithMany(cr => cr.Reviews)
            //    .HasForeignKey(r => r.CraftsmanId)
            //    .OnDelete(DeleteBehavior.Restrict);
            // Reviewer relationship
            builder
                .HasOne(r => r.Reviewer)
                .WithMany(u => u.ReviewsGiven)
                .HasForeignKey(r => r.ReviewerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Target relationship
            builder
                .HasOne(r => r.Target)
                .WithMany(u => u.ReviewsReceived)
                .HasForeignKey(r => r.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);


           
        }
    }
}
