using FluentAssertions;
using SwiftApp.ERP.SharedKernel.Domain;
using Xunit;

namespace SwiftApp.ERP.SharedKernel.Tests;

public class PagedResultTests
{
    [Fact]
    public void ShouldStoreAllProperties()
    {
        var items = new List<string> { "a", "b", "c" };
        var result = new PagedResult<string>(items, Page: 2, PageSize: 10, TotalItems: 25, TotalPages: 3);

        result.Items.Should().BeEquivalentTo(items);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalItems.Should().Be(25);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public void ShouldSupportEmptyResult()
    {
        var result = new PagedResult<int>([], Page: 1, PageSize: 20, TotalItems: 0, TotalPages: 0);

        result.Items.Should().BeEmpty();
        result.TotalItems.Should().Be(0);
    }

    [Fact]
    public void RecordEquality_ShouldWorkCorrectly()
    {
        var a = new PagedResult<int>([1, 2], 1, 10, 2, 1);
        var b = new PagedResult<int>([1, 2], 1, 10, 2, 1);

        a.Should().Be(b);
    }
}
