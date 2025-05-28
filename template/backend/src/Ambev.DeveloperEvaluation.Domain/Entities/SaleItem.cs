using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class SaleItem : BaseEntity
{
    public Guid     SaleId          { get; private set; } 
    public Guid     ProductId       { get; private set; }
    public string   ProductName     { get; private set; }
    public int      Quantity        { get; private set; }
    public decimal  UnitPrice       { get; private set; }
    public decimal  Discount        { get; private set; } 
    public decimal  TotalItemAmount { get; private set; } 
    public bool     IsCancelled     { get; private set; }

    
    public SaleItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        
        if (productId == Guid.Empty)
            throw new DomainException("Product ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("Product name cannot be null or empty.");
        if (unitPrice <= 0)
            throw new DomainException("Unit price must be greater than zero.");

        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        IsCancelled = false;
        
        UpdateQuantity(quantity);
        CalculateTotalItemAmount(); 
    }

   
    private SaleItem() { }

    
    internal void SetSaleId(Guid saleId)
    {
        if (saleId == Guid.Empty)
            throw new DomainException("Sale ID cannot be empty when setting for item.");
        SaleId = saleId;     
    }

    public void UpdateQuantity(int newQuantity)
    {
        
        if (newQuantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");
        if (newQuantity > 20)
            throw new DomainException("It's not possible to sell above 20 identical items.");

        Quantity = newQuantity;
        ApplyDiscount(); 
        CalculateTotalItemAmount(); 
       
    }

    private void ApplyDiscount()
    {
        Discount = 0; 

        if (Quantity >= 10) // Entre 10 e 20 itens: 20% de desconto
        {
            Discount = UnitPrice * Quantity * 0.20m;
        }
        else if (Quantity >= 4) // Acima de 4 itens (e abaixo de 10): 10% de desconto
        {
            Discount = UnitPrice * Quantity * 0.10m;
        }
        // Compras abaixo de 4 itens não têm desconto (Discount continua 0)
    }

    private void CalculateTotalItemAmount()
    {
        TotalItemAmount = (UnitPrice * Quantity) - Discount;
    }

    public void Cancel()
    {
        if (IsCancelled)
            throw new DomainException("Sale item is already cancelled.");

        IsCancelled = true;
    }
}
