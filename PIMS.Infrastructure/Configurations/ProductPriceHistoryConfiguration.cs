using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PIMS.Domain.Entities;

namespace PIMS.Infrastructure.Configurations;

public class ProductPriceHistoryConfiguration
    : IEntityTypeConfiguration<ProductPriceHistory>
{
    public void Configure(EntityTypeBuilder<ProductPriceHistory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OldPrice)
               .HasPrecision(18, 2)
               .IsRequired();

        builder.Property(x => x.NewPrice)
               .HasPrecision(18, 2)
               .IsRequired();

        builder.Property(x => x.Reason)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(x => x.ChangedAt)
               .IsRequired();

        builder.HasOne(x => x.Product)
               .WithMany(p => p.PriceHistories)
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}