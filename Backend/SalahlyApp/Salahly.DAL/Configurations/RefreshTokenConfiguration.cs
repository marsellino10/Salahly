using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salahly.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.CreatedAt)
                .IsRequired();

            builder.Property(e => e.ExpiresAt)
                .IsRequired();

            builder.Property(e => e.RemoteIpAddress)
                .HasMaxLength(100);

            builder.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
