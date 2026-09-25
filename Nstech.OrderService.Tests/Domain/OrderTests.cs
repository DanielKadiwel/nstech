using System;
using FluentAssertions;
using Nstech.OrderService.Domain.Entities;
using Nstech.OrderService.Domain.Enums;
using Nstech.OrderService.Domain.Exceptions;
using Xunit;

namespace Nstech.OrderService.Tests.Domain;

public class OrderTests
{
    [Fact]
    public void Order_Should_Start_In_Draft_Status()
    {
        var order = new Order(Guid.NewGuid(), "BRL");
        order.Status.Should().Be(OrderStatus.Draft);
    }

    [Fact]
    public void Place_Order_Without_Items_Should_Throw_DomainException()
    {
        var order = new Order(Guid.NewGuid(), "BRL");
        
        Action act = () => order.Place();

        act.Should().Throw<DomainException>()
           .WithMessage("Cannot place an order without items.");
    }

    [Fact]
    public void Place_Order_With_Items_Should_Change_Status_To_Placed()
    {
        var order = new Order(Guid.NewGuid(), "BRL");
        order.AddItem(Guid.NewGuid(), 100m, 2);
        
        order.Place();

        order.Status.Should().Be(OrderStatus.Placed);
        order.Total.Should().Be(200m);
    }
}
