namespace NursingPlatform.WebApi.Tests.IntegrationTests;

public class SafeOpenApiCaptureModeTests
{
    [Fact]
    public void FromRawArguments_RemovesExactCaptureSentinelAndPreservesOtherArguments()
    {
        string[] rawArguments = ["--urls=http://localhost:5167", "--capture-openapi", "--environment=Development"];

        var captureMode = OpenApiCaptureMode.FromRawArguments(rawArguments);

        Assert.True(captureMode.IsEnabled);
        Assert.Equal(["--urls=http://localhost:5167", "--environment=Development"], captureMode.RemainingArguments);
    }

    [Fact]
    public void FromRawArguments_DoesNotMatchNonExactCaptureSentinel()
    {
        string[] rawArguments = ["--capture-openapi=true", "capture-openapi", "--urls=http://localhost:5167"];

        var captureMode = OpenApiCaptureMode.FromRawArguments(rawArguments);

        Assert.False(captureMode.IsEnabled);
        Assert.Equal(rawArguments, captureMode.RemainingArguments);
    }

    [Fact]
    public void EnsureDevelopmentOnly_WhenCaptureModeEnabledOutsideDevelopment_ThrowsClearFailure()
    {
        var captureMode = new OpenApiCaptureMode(true, []);

        var exception = Record.Exception(() =>
            captureMode.EnsureDevelopmentOnly(isDevelopment: false));

        var invalidOperationException = Assert.IsType<InvalidOperationException>(exception);
        Assert.Contains(
            "OpenAPI capture mode is Development-only",
            invalidOperationException.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void EnsureDevelopmentOnly_WhenCaptureModeDisabledOutsideDevelopment_DoesNotThrow()
    {
        var captureMode = new OpenApiCaptureMode(false, ["--urls=http://localhost:5167"]);

        captureMode.EnsureDevelopmentOnly(isDevelopment: false);
    }
}
