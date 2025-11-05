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
    public class PortfolioItemConfiguration : IEntityTypeConfiguration<PortfolioItem>
    {
        public void Configure(EntityTypeBuilder<PortfolioItem> builder)
        {
            builder.Property(p => p.Title)
                   .HasMaxLength(150)
                   .IsRequired();

            builder.Property(p => p.Description)
                   .HasMaxLength(500);

            builder.HasOne(p => p.Craftsman)
                   .WithMany(c => c.Portfolio)
                   .HasForeignKey(p => p.CraftsmanId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
