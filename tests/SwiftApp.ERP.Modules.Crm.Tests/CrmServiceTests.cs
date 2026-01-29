using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SwiftApp.ERP.Modules.Crm.Application.DTOs;
using SwiftApp.ERP.Modules.Crm.Application.Services;
using SwiftApp.ERP.Modules.Crm.Domain.Entities;
using SwiftApp.ERP.Modules.Crm.Domain.Events;
using SwiftApp.ERP.Modules.Crm.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using Xunit;

namespace SwiftApp.ERP.Modules.Crm.Tests;

public class ContactServiceTests
{
    private readonly Mock<IContactRepository> _contactRepo = new();
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<ILogger<ContactService>> _logger = new();
    private readonly ContactService _sut;

    public ContactServiceTests()
    {
        _sut = new ContactService(_contactRepo.Object, _mediator.Object, _logger.Object);
    }

    private static Contact CreateTestContact() => new()
    {
        Id = Guid.NewGuid(),
        FirstName = "Marie",
        LastName = "Dubois",
        Email = "marie.dubois@luxwatch.ch",
        Phone = "+41 21 555 1234",
        Company = "LuxWatch SA",
        Position = "Purchasing Manager",
        City = "Lausanne",
        Canton = "VD",
        Country = "CH",
        Active = true,
        CreatedAt = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task ShouldCreateContact()
    {
        var request = new ContactRequest("Marie", "Dubois", "marie@luxwatch.ch", "+41 21 555 1234",
            "LuxWatch SA", "Purchasing Manager", null, "Lausanne", "1000", "VD", "CH", null, null, true);

        var result = await _sut.CreateAsync(request);

        result.FirstName.Should().Be("Marie");
        result.LastName.Should().Be("Dubois");
        result.Country.Should().Be("CH");
        _contactRepo.Verify(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediator.Verify(m => m.Publish(It.IsAny<ContactCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldReturnContact_WhenFound()
    {
        var contact = CreateTestContact();
        _contactRepo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);

        var result = await _sut.GetByIdAsync(contact.Id);

        result.Should().NotBeNull();
        result!.DisplayName.Should().Be("Marie Dubois");
    }

    [Fact]
    public async Task ShouldReturnNull_WhenContactNotFound()
    {
        _contactRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Contact?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task ShouldUpdateContact()
    {
        var contact = CreateTestContact();
        _contactRepo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);

        var request = new ContactRequest("Marie", "Dubois-Rochat", "marie@new.ch", null,
            "LuxWatch SA", "Head of Procurement", null, "Genève", null, "GE", "CH", null, null, true);

        var result = await _sut.UpdateAsync(contact.Id, request);

        result.LastName.Should().Be("Dubois-Rochat");
        _contactRepo.Verify(r => r.UpdateAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldThrow_WhenUpdatingNonExistentContact()
    {
        _contactRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Contact?)null);

        var act = () => _sut.UpdateAsync(Guid.NewGuid(), new ContactRequest("A", "B", null, null, null, null, null, null, null, null, null, null, null, null));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ShouldSoftDelete_WhenContactFound()
    {
        var contact = CreateTestContact();
        _contactRepo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);

        await _sut.DeleteAsync(contact.Id);

        _contactRepo.Verify(r => r.SoftDeleteAsync(contact.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldThrow_WhenDeletingNonExistentContact()
    {
        _contactRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Contact?)null);

        var act = () => _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ShouldReturnContactsByCustomer()
    {
        var customerId = Guid.NewGuid();
        var contacts = new List<Contact> { CreateTestContact(), CreateTestContact() };
        _contactRepo.Setup(r => r.GetByCustomerIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contacts);

        var result = await _sut.GetByCustomerIdAsync(customerId);

        result.Should().HaveCount(2);
    }
}
