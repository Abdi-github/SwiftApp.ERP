using FluentAssertions;
using SwiftApp.ERP.SharedKernel.Exceptions;
using Xunit;

namespace SwiftApp.ERP.SharedKernel.Tests;

public class ExceptionTests
{
    [Fact]
    public void EntityNotFoundException_ShouldIncludeEntityNameAndId()
    {
        var id = Guid.NewGuid();
        var ex = new EntityNotFoundException("Product", id);

        ex.EntityName.Should().Be("Product");
        ex.EntityId.Should().Be(id);
        ex.Message.Should().Contain("Product").And.Contain(id.ToString());
    }

    [Fact]
    public void BusinessRuleException_ShouldIncludeRuleAndMessage()
    {
        var ex = new BusinessRuleException("BALANCE_CHECK", "Debits must equal credits");

        ex.Rule.Should().Be("BALANCE_CHECK");
        ex.Message.Should().Be("Debits must equal credits");
    }

    [Fact]
    public void ConcurrencyException_ShouldIncludeEntityNameAndId()
    {
        var id = Guid.NewGuid();
        var ex = new ConcurrencyException("SalesOrder", id);

        ex.EntityName.Should().Be("SalesOrder");
        ex.EntityId.Should().Be(id);
        ex.Message.Should().Contain("SalesOrder").And.Contain("concurrency", StringComparison.OrdinalIgnoreCase);
    }
}
