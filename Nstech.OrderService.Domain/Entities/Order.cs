using System;
using System.Collections.Generic;
using System.Linq;
using Nstech.OrderService.Domain.Enums;
using Nstech.OrderService.Domain.Exceptions;

namespace Nstech.OrderService.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public string Currency { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public decimal Total => _items.Sum(i => i.GetTotal());

    protected Order() { }

    public Order(Guid customerId, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency)) throw new DomainException("Currency is required.");
        
        Id = Guid.NewGuid();
        CustomerId = customerId;
        Currency = currency;
        Status = OrderStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddItem(Guid productId, decimal unitPrice, int quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Cannot add items to an order that is not in Draft status.");

        var item = new OrderItem(productId, unitPrice, quantity);
        _items.Add(item);
    }

    public void Place()
    {
        if (!_items.Any()) throw new DomainException("Cannot place an order without items.");
        if (Status != OrderStatus.Draft) throw new DomainException("Only Draft orders can be placed.");

        Status = OrderStatus.Placed;
    }

    public void Confirm()
    {
        if (Status == OrderStatus.Confirmed) return; // Idempotency
        if (Status != OrderStatus.Placed) throw new DomainException("Only Placed orders can be confirmed.");

        Status = OrderStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Canceled) return; // Idempotency
        if (Status != OrderStatus.Placed && Status != OrderStatus.Confirmed)
            throw new DomainException("Only Placed or Confirmed orders can be canceled.");

        Status = OrderStatus.Canceled;
    }
}
