using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface ISaleRepository
{
    Task AddAsync(Sale sale, CancellationToken cancellationToken);
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task UpdateAsync(Sale sale, CancellationToken cancellationToken);
    Task<IEnumerable<Sale>> GetAllAsync(CancellationToken cancellationToken);
    Task<bool> SaleNumberExistsAsync(string saleNumber, CancellationToken cancellationToken);
}