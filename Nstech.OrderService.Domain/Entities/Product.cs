using System;

namespace Nstech.OrderService.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int AvailableQuantity { get; private set; }

    protected Product() { } // For EF Core

    public Product(Guid id, string name, decimal unitPrice, int availableQuantity)
    {
        Id = id;
        Name = name;
        UnitPrice = unitPrice;
        AvailableQuantity = availableQuantity;
    }

    public void ReserveStock(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (AvailableQuantity < quantity) throw new Exceptions.DomainException($"Insufficient stock for product {Id}.");
        
        AvailableQuantity -= quantity;
    }

    public void ReleaseStock(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        AvailableQuantity += quantity;
    }
}
