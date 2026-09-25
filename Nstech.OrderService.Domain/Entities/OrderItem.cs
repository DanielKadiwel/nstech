using System;

namespace Nstech.OrderService.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    protected OrderItem() { }

    public OrderItem(Guid productId, decimal unitPrice, int quantity)
    {
        if (quantity <= 0) throw new Exceptions.DomainException("Quantity must be greater than zero.");
        if (unitPrice < 0) throw new Exceptions.DomainException("Unit price cannot be negative.");

        Id = Guid.NewGuid();
        ProductId = productId;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public decimal GetTotal() => UnitPrice * Quantity;
}
