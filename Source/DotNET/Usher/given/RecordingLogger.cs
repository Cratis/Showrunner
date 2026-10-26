// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Usher.given;

/// <summary>
/// A logger that keeps what is logged, so a specification can see that something was logged and how.
/// </summary>
/// <typeparam name="T">The category of the logger.</typeparam>
public class RecordingLogger<T> : ILogger<T>
{
    readonly List<LoggedMessage> _messages = [];

    /// <summary>
    /// Gets the messages logged so far.
    /// </summary>
    public IReadOnlyList<LoggedMessage> Messages
    {
        get
        {
            lock (_messages)
            {
                return [.. _messages];
            }
        }
    }

    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel) => true;

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        lock (_messages)
        {
            _messages.Add(new(logLevel, eventId.Name ?? string.Empty, formatter(state, exception)));
        }
    }
}

/// <summary>
/// A message a <see cref="RecordingLogger{T}"/> kept.
/// </summary>
/// <param name="Level">The level it was logged at.</param>
/// <param name="Name">The name of the log message, which is the name of the method that logs it.</param>
/// <param name="Text">The formatted text.</param>
public record LoggedMessage(LogLevel Level, string Name, string Text);
#endif
