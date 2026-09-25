using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Nstech.OrderService.Application.DTOs;
using Nstech.OrderService.Domain.Enums;
using Nstech.OrderService.Domain.Interfaces;

namespace Nstech.OrderService.Application.Queries;

public class GetOrdersQuery : IRequest<PagedResult<OrderDto>>
{
    public Guid? CustomerId { get; set; }
    public string Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        OrderStatus? parsedStatus = null;
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<OrderStatus>(request.Status, true, out var st))
        {
            parsedStatus = st;
        }

        var (orders, totalCount) = await _orderRepository.GetPagedAsync(
            request.CustomerId, parsedStatus, request.From, request.To, request.Page, request.PageSize, cancellationToken);

        var items = orders.Select(order => new OrderDto
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
        }).ToList();

        return new PagedResult<OrderDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
