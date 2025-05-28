using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        
        builder.HasKey(s => s.Id);

        builder.Property(s => s.SaleNumber)
            .IsRequired()
            .HasMaxLength(50); 

    builder.Property(s => s.SaleDate)
        .IsRequired()
        .HasColumnType("timestamp with time zone");

    builder.Property(s => s.CustomerId)
            .IsRequired(); 

        builder.Property(s => s.CustomerName)
            .IsRequired()
            .HasMaxLength(255); 

        builder.Property(s => s.BranchId)
            .IsRequired(); 

        builder.Property(s => s.BranchName)
            .IsRequired()
            .HasMaxLength(255); 

        builder.Property(s => s.TotalAmount)
            .HasColumnType("numeric(18,2)") 
            .IsRequired();

        builder.Property(s => s.IsCancelled)
            .IsRequired();
        
        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone"); 
        builder.Property(s => s.UpdatedAt)
            .HasColumnType("timestamp with time zone"); 

        builder.HasMany(s => s.Items) 
            .WithOne() 
            .HasForeignKey(si => si.SaleId) 
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
       
        builder.HasIndex(s => s.SaleNumber).IsUnique();
       
        builder.ToTable("Sales");
    }
}
