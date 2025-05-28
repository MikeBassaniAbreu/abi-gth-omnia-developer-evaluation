using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        
        builder.HasKey(si => si.Id);

        builder.Property(si => si.SaleId)
            .IsRequired(); 

        builder.Property(si => si.ProductId)
            .IsRequired();

        builder.Property(si => si.ProductName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(si => si.Quantity)
            .IsRequired();

        builder.Property(si => si.UnitPrice)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(si => si.Discount)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(si => si.TotalItemAmount)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(si => si.IsCancelled)
            .IsRequired();

        builder.Property(si => si.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");
        builder.Property(si => si.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        builder.ToTable("SaleItems");
    }
}
