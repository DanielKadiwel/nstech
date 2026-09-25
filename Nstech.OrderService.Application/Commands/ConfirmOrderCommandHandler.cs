using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Nstech.OrderService.Domain.Exceptions;
using Nstech.OrderService.Domain.Interfaces;

namespace Nstech.OrderService.Application.Commands;

public class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        
        if (order == null)
            throw new DomainException("Order not found.");

        if (order.Status == Domain.Enums.OrderStatus.Confirmed)
            return true; // Idempotent success

        // Requirement: "Confirmação deve reservar/baixar estoque"
        var productIds = order.Items.Select(x => x.ProductId).Distinct();
        var products = await _productRepository.GetByIdsAsync(productIds, cancellationToken);

        foreach (var item in order.Items)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product == null) throw new DomainException($"Product {item.ProductId} not found.");
            
            product.ReserveStock(item.Quantity);
        }

        order.Confirm();

        _productRepository.UpdateRange(products);
        _orderRepository.Update(order);
        
        await _unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}
