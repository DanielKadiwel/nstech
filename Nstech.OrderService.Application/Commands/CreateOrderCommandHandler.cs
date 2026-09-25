using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Nstech.OrderService.Domain.Entities;
using Nstech.OrderService.Domain.Exceptions;
using Nstech.OrderService.Domain.Interfaces;

namespace Nstech.OrderService.Application.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.Items == null || !request.Items.Any())
            throw new DomainException("Cannot create order without items.");

        var productIds = request.Items.Select(x => x.ProductId).Distinct().ToList();
        var products = await _productRepository.GetByIdsAsync(productIds, cancellationToken);

        if (products.Count() != productIds.Count)
            throw new DomainException("One or more products do not exist.");

        var order = new Order(request.CustomerId, request.Currency);

        foreach (var itemCommand in request.Items)
        {
            var product = products.First(p => p.Id == itemCommand.ProductId);
            
            // Validate stock logic can be here or in Confirm depending on business rules.
            // Requirement says: "Não pode exceder estoque disponível (conforme seu modelo)" on POST /orders
            if (product.AvailableQuantity < itemCommand.Quantity)
                throw new DomainException($"Insufficient stock for product {product.Id}");

            order.AddItem(product.Id, product.UnitPrice, itemCommand.Quantity);
        }

        order.Place(); // Requirement: "pedido nasce como Placed"

        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return order.Id;
    }
}
