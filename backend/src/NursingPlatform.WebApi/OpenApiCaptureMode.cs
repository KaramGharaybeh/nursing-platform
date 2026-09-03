namespace NursingPlatform.WebApi;

internal readonly record struct OpenApiCaptureMode(bool IsEnabled, string[] RemainingArguments)
{
    private const string Sentinel = "--capture-openapi";

    public static OpenApiCaptureMode FromRawArguments(string[] rawArguments)
    {
        var isEnabled = false;
        var remainingArguments = new List<string>(rawArguments.Length);

        foreach (var argument in rawArguments)
        {
            if (string.Equals(argument, Sentinel, StringComparison.Ordinal))
            {
                isEnabled = true;
                continue;
            }

            remainingArguments.Add(argument);
        }

        return new OpenApiCaptureMode(isEnabled, [.. remainingArguments]);
    }

    public void EnsureDevelopmentOnly(bool isDevelopment)
    {
        if (IsEnabled && !isDevelopment)
        {
            throw new InvalidOperationException("OpenAPI capture mode is Development-only.");
        }
    }
}
