using FluentAssertions;
using SwiftApp.ERP.SharedKernel.Domain;
using Xunit;

namespace SwiftApp.ERP.SharedKernel.Tests;

public class VatRateTests
{
    [Theory]
    [InlineData(VatRate.Standard, 8.1)]
    [InlineData(VatRate.Reduced, 2.6)]
    [InlineData(VatRate.Accommodation, 3.8)]
    [InlineData(VatRate.Exempt, 0)]
    public void Rate_ShouldReturnCorrectPercentage(VatRate vatRate, decimal expected)
    {
        vatRate.Rate().Should().Be(expected);
    }

    [Theory]
    [InlineData(VatRate.Standard, 0.081)]
    [InlineData(VatRate.Reduced, 0.026)]
    [InlineData(VatRate.Accommodation, 0.038)]
    [InlineData(VatRate.Exempt, 0)]
    public void Multiplier_ShouldReturnCorrectDecimal(VatRate vatRate, decimal expected)
    {
        vatRate.Multiplier().Should().Be(expected);
    }

    [Fact]
    public void Rate_ShouldThrow_ForInvalidValue()
    {
        var invalid = (VatRate)99;
        var act = () => invalid.Rate();
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
