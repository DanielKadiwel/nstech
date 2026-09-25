using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nstech.OrderService.Domain.Entities;
using Nstech.OrderService.Domain.Interfaces;

namespace Nstech.OrderService.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _productRepository.GetAllAsync();
        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Product name is required.");

        if (dto.UnitPrice <= 0)
            return BadRequest("Unit price must be greater than zero.");

        if (dto.AvailableQuantity < 0)
            return BadRequest("Available quantity cannot be negative.");

        var product = new Product(Guid.NewGuid(), dto.Name, dto.UnitPrice, dto.AvailableQuantity);
        await _productRepository.AddAsync(product);
        await _unitOfWork.CommitAsync();

        return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
    }
}

public class CreateProductDto
{
    public string Name { get; set; }
    public decimal UnitPrice { get; set; }
    public int AvailableQuantity { get; set; }
}
