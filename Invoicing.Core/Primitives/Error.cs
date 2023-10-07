namespace Invoicing.Core.Primitives;

public record Error(string Code, string Message)
{
    public override string ToString() => $"Code: {Code}. Message: {Message}";
}

public record ManyErrors : Error
{
    private readonly List<Error> _errors;

    public IReadOnlyList<Error> Errors => _errors.AsReadOnly();

    public ManyErrors(IEnumerable<Error> errors) : base("ManyErrors", "See inner errors for details")
    {
        if (errors is null || !errors.Any())
        {
            throw new ArgumentException("Error collection cannot be null or empty");
        }

        _errors = new List<Error>();

        foreach (Error error in errors)
        {
            Add(error);
        }
    }

    public void Add(Error error)
    {
        if (error is ManyErrors many)
        {
            _errors.AddRange(many.Errors);
        }
        else
        {
            _errors.Add(error);
        }
    }

    public override string ToString() => $"Code: {Code}. Message: {Message}";
}
