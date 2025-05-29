using Ambev.DeveloperEvaluation.Common.Validation;
using System;

namespace Ambev.DeveloperEvaluation.Domain.Common;

public abstract class BaseEntity : IComparable<BaseEntity>
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; protected set; } 
    public DateTime? UpdatedAt { get; protected set; } 

    
    protected BaseEntity()
    {
        Id = Guid.NewGuid(); 
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    protected void UpdateLastModified()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public Task<IEnumerable<ValidationErrorDetail>> ValidateAsync()
    {
        return Validator.ValidateAsync(this);
    }

    public int CompareTo(BaseEntity? other)
    {
        if (other == null)
        {
            return 1;
        }

        return other!.Id.CompareTo(Id);
    }
}