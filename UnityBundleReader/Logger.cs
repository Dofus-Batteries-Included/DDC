using Microsoft.Extensions.Logging;

namespace UnityBundleReader;

public static class Logger
{
    static ILogger? _logger;

    public static void Configure(ILogger logger) => _logger = logger;

    public static void Verbose(string message) => _logger?.LogTrace(message);
    public static void Debug(string message) => _logger?.LogDebug(message);
    public static void Info(string message) => _logger?.LogInformation(message);
    public static void Warning(string message) => _logger?.LogWarning(message);
    public static void Error(string message) => _logger?.LogError(message);
    public static void Error(Exception exn, string message) => _logger?.LogError(exn, message);
}
