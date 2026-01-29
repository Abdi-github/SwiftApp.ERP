using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SwiftApp.ERP.Modules.MasterData.Application.DTOs;
using SwiftApp.ERP.Modules.MasterData.Application.Services;
using SwiftApp.ERP.Modules.MasterData.Domain.Entities;
using SwiftApp.ERP.Modules.MasterData.Domain.Events;
using SwiftApp.ERP.Modules.MasterData.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using Xunit;

namespace SwiftApp.ERP.Modules.MasterData.Tests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IPublisher> _publisher = new();
    private readonly Mock<ILogger<ProductService>> _logger = new();
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _sut = new ProductService(_productRepo.Object, _publisher.Object, _logger.Object);
    }

    private static Product CreateTestProduct(string sku = "WATCH-001") => new()
    {
        Id = Guid.NewGuid(),
        Sku = sku,
        Name = "Chronograph Classic",
        Description = "Swiss automatic chronograph",
        UnitPrice = 4500.00m,
        ListPrice = 5200.00m,
        VatRate = VatRate.Standard,
        Active = true,
        CreatedAt = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task ShouldReturnProduct_WhenFoundById()
    {
        var product = CreateTestProduct();
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _sut.GetByIdAsync(product.Id);

        result.Should().NotBeNull();
        result!.Sku.Should().Be("WATCH-001");
        result.UnitPrice.Should().Be(4500.00m);
    }

    [Fact]
    public async Task ShouldReturnNull_WhenProductNotFound()
    {
        _productRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task ShouldReturnProduct_WhenFoundBySku()
    {
        var product = CreateTestProduct();
        _productRepo.Setup(r => r.GetBySkuAsync("WATCH-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _sut.GetBySkuAsync("WATCH-001");

        result.Should().NotBeNull();
        result!.Name.Should().Be("Chronograph Classic");
    }

    [Fact]
    public async Task ShouldCreateProduct_WhenSkuUnique()
    {
        var request = new ProductRequest("WATCH-002", "Diver Pro", "Swiss diver watch", null, 3800.00m, 4200.00m, VatRate.Standard, true, null, null);

        _productRepo.Setup(r => r.GetBySkuAsync("WATCH-002", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await _sut.CreateAsync(request);

        result.Sku.Should().Be("WATCH-002");
        result.Name.Should().Be("Diver Pro");
        _productRepo.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _publisher.Verify(p => p.Publish(It.IsAny<ProductCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldThrow_WhenSkuAlreadyExists()
    {
        var request = new ProductRequest("WATCH-001", "Duplicate", null, null, 100m, 100m, VatRate.Standard, true, null, null);
        _productRepo.Setup(r => r.GetBySkuAsync("WATCH-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestProduct());

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.Rule == "UNIQUE_SKU");
    }

    [Fact]
    public async Task ShouldUpdateProduct_WhenFound()
    {
        var product = CreateTestProduct();
        var request = new ProductRequest("WATCH-001", "Updated Name", "Updated desc", null, 5000m, 5500m, VatRate.Standard, true, null, null);

        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _sut.UpdateAsync(product.Id, request);

        result.Name.Should().Be("Updated Name");
        _productRepo.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _publisher.Verify(p => p.Publish(It.IsAny<ProductUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldThrow_WhenUpdatingNonExistentProduct()
    {
        _productRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var act = () => _sut.UpdateAsync(Guid.NewGuid(), new ProductRequest("X", "X", null, null, 1m, 1m, VatRate.Standard, true, null, null));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ShouldSoftDeleteProduct_WhenFound()
    {
        var product = CreateTestProduct();
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        await _sut.DeleteAsync(product.Id);

        _productRepo.Verify(r => r.SoftDeleteAsync(product.Id, It.IsAny<CancellationToken>()), Times.Once);
        _publisher.Verify(p => p.Publish(It.IsAny<ProductDeletedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldThrow_WhenDeletingNonExistentProduct()
    {
        _productRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var act = () => _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ShouldReturnPagedResults()
    {
        var products = new List<Product> { CreateTestProduct("W-1"), CreateTestProduct("W-2") };
        _productRepo.Setup(r => r.GetPagedAsync(1, 20, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Product>(products, 1, 20, 2, 1));

        var result = await _sut.GetPagedAsync(1, 20);

        result.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(2);
    }

    [Fact]
    public async Task ShouldReturnTrue_WhenProductActive()
    {
        var product = CreateTestProduct();
        product.Active = true;
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _sut.IsActiveAsync(product.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldReturnFalse_WhenProductInactive()
    {
        var product = CreateTestProduct();
        product.Active = false;
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _sut.IsActiveAsync(product.Id);

        result.Should().BeFalse();
    }
}
