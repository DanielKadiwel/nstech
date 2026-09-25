using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Nstech.OrderService.Domain.Exceptions;
using Nstech.OrderService.Domain.Interfaces;

namespace Nstech.OrderService.Application.Commands;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        
        if (order == null)
            throw new DomainException("Order not found.");

        if (order.Status == Domain.Enums.OrderStatus.Canceled)
            return true; // Idempotent success

        bool wasConfirmed = order.Status == Domain.Enums.OrderStatus.Confirmed;

        order.Cancel();

        if (wasConfirmed)
        {
            // Reverse stock reservation
            var productIds = order.Items.Select(x => x.ProductId).Distinct();
            var products = await _productRepository.GetByIdsAsync(productIds, cancellationToken);

            foreach (var item in order.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    product.ReleaseStock(item.Quantity);
                }
            }
            _productRepository.UpdateRange(products);
        }

        _orderRepository.Update(order);
        await _unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}
