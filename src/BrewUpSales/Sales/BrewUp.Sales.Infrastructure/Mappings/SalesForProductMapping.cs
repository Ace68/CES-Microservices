using BrewUp.Sales.Entities.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrewUp.Sales.Infrastructure.Mappings;

public class SalesForProductMapping : IEntityTypeConfiguration<SalesForProduct>
{
    public void Configure(EntityTypeBuilder<SalesForProduct> builder)
    {
        builder.ToTable("SalesForProduct", "dbo");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(36);
        builder.Property(x => x.ProductName)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.Quantity)
            .IsRequired();
        builder.Property(x => x.UnitOfMeasure)
            .IsRequired()
            .HasMaxLength(10);
        builder.Property(x => x.Price)
            .IsRequired();
        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(3);
    }
}