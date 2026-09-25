using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nstech.OrderService.Domain.Entities;

namespace Nstech.OrderService.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    void Update(Product product);
    void UpdateRange(IEnumerable<Product> products);
}
