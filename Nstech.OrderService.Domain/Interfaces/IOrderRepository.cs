using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nstech.OrderService.Domain.Entities;
using Nstech.OrderService.Domain.Enums;

namespace Nstech.OrderService.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    void Update(Order order);
    
    // For pagination/filtering
    Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedAsync(
        Guid? customerId, 
        OrderStatus? status, 
        DateTime? from, 
        DateTime? to, 
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default);
}
