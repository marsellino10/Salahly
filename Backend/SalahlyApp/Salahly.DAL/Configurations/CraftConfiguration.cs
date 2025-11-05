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
    public class CraftConfiguration : IEntityTypeConfiguration<Craft>
    {
        public void Configure(EntityTypeBuilder<Craft> builder)
        {
            builder.Property(c => c.Name)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(c => c.Description)
                   .HasMaxLength(500);
        }
    }
}
