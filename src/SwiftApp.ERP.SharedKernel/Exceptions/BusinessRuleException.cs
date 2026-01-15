namespace SwiftApp.ERP.SharedKernel.Exceptions;

/// <summary>
/// Thrown when a business rule is violated.
/// </summary>
public sealed class BusinessRuleException : Exception
{
    public string Rule { get; }

    public BusinessRuleException(string rule, string message)
        : base(message)
    {
        Rule = rule;
    }
}
