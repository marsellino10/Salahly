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
    public class CraftsmanConfiguration : IEntityTypeConfiguration<Craftsman>
    {
        public void Configure(EntityTypeBuilder<Craftsman> builder)
        {
            builder.HasKey(c => c.Id);

            builder.HasOne(c => c.User)
                   .WithOne()
                   .HasForeignKey<Craftsman>(c => c.Id)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Craft)
                   .WithMany(ca => ca.Craftsmen)
                   .HasForeignKey(c => c.CraftId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(c => c.RatingAverage)
                   .HasPrecision(3, 2)
                   .HasDefaultValue(0);
        }
    }
}
