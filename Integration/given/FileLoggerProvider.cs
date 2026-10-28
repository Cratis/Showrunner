// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Usher.Integration.given;

/// <summary>
/// Writes the application's log to a file, so a failing run can be diagnosed. Used when <c>USHER_INTEGRATION_LOG</c>
/// names the file.
/// </summary>
/// <param name="path">The file to append to.</param>
public sealed class FileLoggerProvider(string path) : ILoggerProvider
{
    readonly Lock _lock = new();

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName) => new FileLogger(this, categoryName);

    /// <inheritdoc/>
    public void Dispose()
    {
    }

    void Write(string line)
    {
        lock (_lock)
        {
            File.AppendAllText(path, line + Environment.NewLine);
        }
    }

    sealed class FileLogger(FileLoggerProvider provider, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                provider.Write($"{DateTimeOffset.UtcNow:O} {logLevel} {category}: {formatter(state, exception)}{(exception is null ? string.Empty : Environment.NewLine + exception)}");
            }
        }
    }
}
