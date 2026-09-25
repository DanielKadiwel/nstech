using System;
using System.Collections.Generic;
using MediatR;

namespace Nstech.OrderService.Application.Commands;

public class CreateOrderCommand : IRequest<Guid>
{
    public Guid CustomerId { get; set; }
    public string Currency { get; set; }
    public List<CreateOrderItemCommand> Items { get; set; } = new();
}

public class CreateOrderItemCommand
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
