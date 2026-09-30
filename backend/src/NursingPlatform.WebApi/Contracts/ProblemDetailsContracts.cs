namespace NursingPlatform.WebApi.Contracts;

public class ProblemDetailsContract
{
    public string Type { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public int Status { get; init; }
    public string Detail { get; init; } = string.Empty;
    public string TraceId { get; init; } = string.Empty;
}

public class ValidationProblemDetailsContract : ProblemDetailsContract
{
    public IReadOnlyDictionary<string, string[]> Errors { get; init; } =
        new Dictionary<string, string[]>();
}

public class CodedProblemDetailsContract : ProblemDetailsContract
{
    public string Code { get; init; } = string.Empty;
}

public class RetryableProblemDetailsContract : ProblemDetailsContract
{
    public int RetryAfterSeconds { get; init; }
}
