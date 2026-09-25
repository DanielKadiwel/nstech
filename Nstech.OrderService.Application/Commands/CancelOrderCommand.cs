using System;
using MediatR;

namespace Nstech.OrderService.Application.Commands;

public class CancelOrderCommand : IRequest<bool>
{
    public Guid OrderId { get; set; }

    public CancelOrderCommand(Guid orderId)
    {
        OrderId = orderId;
    }
}
