using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salahly.DAL.Enteties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.Property(b => b.Price)
                   .HasColumnType("decimal(10,2)")
                   .IsRequired();

            builder.HasOne(b => b.Customer)
                   .WithMany(c => c.Bookings)
                   .HasForeignKey(b => b.CustomerId);

            builder.HasOne(b => b.Craftsman)
                   .WithMany(c => c.Bookings)
                   .HasForeignKey(b => b.CraftsmanId);

            builder.HasOne(b => b.Craft)
                   .WithMany()
                   .HasForeignKey(b => b.CraftId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
