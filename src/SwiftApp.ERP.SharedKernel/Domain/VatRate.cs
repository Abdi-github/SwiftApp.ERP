namespace SwiftApp.ERP.SharedKernel.Domain;

/// <summary>
/// Swiss VAT rates as defined by the Federal Tax Administration.
/// </summary>
public enum VatRate
{
    /// <summary>Standard rate: 8.1%</summary>
    Standard,

    /// <summary>Reduced rate: 2.6%</summary>
    Reduced,

    /// <summary>Accommodation rate: 3.8%</summary>
    Accommodation,

    /// <summary>Exempt: 0%</summary>
    Exempt
}

public static class VatRateExtensions
{
    public static decimal Rate(this VatRate vatRate) => vatRate switch
    {
        VatRate.Standard => 8.1m,
        VatRate.Reduced => 2.6m,
        VatRate.Accommodation => 3.8m,
        VatRate.Exempt => 0m,
        _ => throw new ArgumentOutOfRangeException(nameof(vatRate))
    };

    public static decimal Multiplier(this VatRate vatRate) =>
        Math.Round(vatRate.Rate() / 100m, 4, MidpointRounding.AwayFromZero);
}
