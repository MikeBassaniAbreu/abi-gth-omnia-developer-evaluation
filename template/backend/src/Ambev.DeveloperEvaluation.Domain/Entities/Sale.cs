using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Sale : BaseEntity
{
    public string   SaleNumber   { get; private set; }
    public DateTime SaleDate     { get; private set; }
    public Guid     CustomerId   { get; private set; } 
    public string   CustomerName { get; private set; } 
    public Guid     BranchId     { get; private set; } 
    public string   BranchName   { get; private set; } 
    public decimal  TotalAmount  { get; private set; }
    public bool     IsCancelled  { get; private set; }

    
    private readonly List<SaleItem> _items = new();
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    public Sale(string saleNumber, DateTime saleDate, Guid customerId, string customerName, Guid branchId, string branchName)
    {
        if (string.IsNullOrWhiteSpace(saleNumber))
            throw new DomainException("Sale number cannot be null or empty.");
        if (saleDate == default)
            throw new DomainException("Sale date cannot be default.");
        if (customerId == Guid.Empty)
            throw new DomainException("Customer ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("Customer name cannot be null or empty.");
        if (branchId == Guid.Empty)
            throw new DomainException("Branch ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(branchName))
            throw new DomainException("Branch name cannot be null or empty.");

        SaleNumber = saleNumber;
        SaleDate = saleDate;
        CustomerId = customerId;
        CustomerName = customerName;
        BranchId = branchId;
        BranchName = branchName;
        TotalAmount = 0; 
        IsCancelled = false;            
    }

    private Sale() { }

    public void UpdateSaleDetails(Guid customerId, string customerName, Guid branchId, string branchName, DateTime saleDate)
    {
        if (customerId == Guid.Empty)
            throw new DomainException("Customer ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("Customer name cannot be null or empty.");
        if (branchId == Guid.Empty)
            throw new DomainException("Branch ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(branchName))
            throw new DomainException("Branch name cannot be null or empty.");
        if (saleDate == default)
            throw new DomainException("Sale date cannot be default.");
        if (saleDate > DateTime.UtcNow)
            throw new DomainException("Sale date cannot be in the future.");

        CustomerId = customerId;
        CustomerName = customerName;
        BranchId = branchId;
        BranchName = branchName;
        SaleDate = saleDate;

        UpdateLastModified(); 
    }

    public void AddItem(SaleItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item), "Sale item cannot be null.");

        item.SetSaleId(Id);

        _items.Add(item);
        CalculateTotalAmount(); 
    }

    public void RemoveItem(Guid itemId)
    {
        var itemToRemove = _items.FirstOrDefault(i => i.Id == itemId);
        if (itemToRemove == null)
            throw new DomainException($"Sale item with ID {itemId} not found in this sale.");

        _items.Remove(itemToRemove);
        CalculateTotalAmount(); 
    }

    public void CalculateTotalAmount()
    {
        TotalAmount = _items.Sum(item => item.TotalItemAmount);
      
        UpdateLastModified();
    }

    public void Cancel()
    {
        if (IsCancelled)
            throw new DomainException("Sale is already cancelled.");

        IsCancelled = true;
        UpdateLastModified();
    }

   
    private new void UpdateLastModified()
    {
        base.UpdateLastModified(); 
    }
}
