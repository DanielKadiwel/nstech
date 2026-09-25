using System;
using MediatR;

namespace Nstech.OrderService.Application.Commands;

public class ConfirmOrderCommand : IRequest<bool>
{
    public Guid OrderId { get; set; }

    public ConfirmOrderCommand(Guid orderId)
    {
        OrderId = orderId;
    }
}
