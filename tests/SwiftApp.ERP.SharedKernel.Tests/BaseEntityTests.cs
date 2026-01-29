using FluentAssertions;
using SwiftApp.ERP.SharedKernel.Domain;
using Xunit;

namespace SwiftApp.ERP.SharedKernel.Tests;

public class BaseEntityTests
{
    private class TestEntity : BaseEntity { }

    [Fact]
    public void NewEntity_ShouldHaveGeneratedId()
    {
        var entity = new TestEntity();
        entity.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void IsDeleted_ShouldReturnFalse_WhenDeletedAtIsNull()
    {
        var entity = new TestEntity { DeletedAt = null };
        entity.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void IsDeleted_ShouldReturnTrue_WhenDeletedAtIsSet()
    {
        var entity = new TestEntity { DeletedAt = DateTimeOffset.UtcNow };
        entity.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void TwoNewEntities_ShouldHaveDifferentIds()
    {
        var a = new TestEntity();
        var b = new TestEntity();
        a.Id.Should().NotBe(b.Id);
    }
}
