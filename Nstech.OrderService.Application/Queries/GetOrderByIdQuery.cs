using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Nstech.OrderService.Application.DTOs;
using Nstech.OrderService.Domain.Interfaces;

namespace Nstech.OrderService.Application.Queries;

public class GetOrderByIdQuery : IRequest<OrderDto>
{
    public Guid OrderId { get; set; }

    public GetOrderByIdQuery(Guid orderId)
    {
        OrderId = orderId;
    }
}

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null) return null;

        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status.ToString(),
            Currency = order.Currency,
            Total = order.Total,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity
            }).ToList()
        };
    }
}
