// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Cratis.DependencyInjection;
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Usher.Reporting.Capture;

/// <summary>
/// Represents the in-memory <see cref="ICapturedErrors"/>, bounded by <see cref="ErrorCaptureOptions.Capacity"/> and
/// <see cref="ErrorCaptureOptions.Retention"/>.
/// </summary>
/// <remarks>
/// The store is per process: a report that reaches another instance than the one the error happened on finds nothing
/// and is recorded from what the client sends, as it is without capture.
/// </remarks>
/// <param name="options">The <see cref="UsherOptions"/> holding the capture options.</param>
/// <param name="clock">The <see cref="TimeProvider"/> captured errors expire by.</param>
[Singleton]
public class CapturedErrors(IOptions<UsherOptions> options, TimeProvider clock) : ICapturedErrors
{
    readonly Lock _lock = new();
    readonly Dictionary<(ErrorOwner Owner, CorrelationId CorrelationId), (CapturedError Error, DateTimeOffset CapturedAt)> _errors = [];
    readonly LinkedList<(ErrorOwner Owner, CorrelationId CorrelationId)> _order = [];

    /// <inheritdoc/>
    public void Capture(ErrorOwner owner, CorrelationId correlationId, CapturedError error)
    {
        var capture = options.Value.ErrorCapture;
        if (!capture.Enabled || capture.Capacity <= 0)
        {
            return;
        }

        var now = clock.GetUtcNow();
        lock (_lock)
        {
            RemoveExpired(now, capture.Retention);
            var key = (owner, correlationId);
            if (!_errors.TryAdd(key, (error, now)))
            {
                return;
            }

            _order.AddLast(key);
            while (_errors.Count > capture.Capacity)
            {
                _errors.Remove(_order.First!.Value);
                _order.RemoveFirst();
            }
        }
    }

    /// <inheritdoc/>
    public bool TryGet(ErrorOwner owner, CorrelationId correlationId, [NotNullWhen(true)] out CapturedError? error)
    {
        var now = clock.GetUtcNow();
        lock (_lock)
        {
            RemoveExpired(now, options.Value.ErrorCapture.Retention);
            if (_errors.TryGetValue((owner, correlationId), out var captured))
            {
                error = captured.Error;
                return true;
            }
        }

        error = null;
        return false;
    }

    void RemoveExpired(DateTimeOffset now, TimeSpan retention)
    {
        while (_order.First is { } oldest && now - _errors[oldest.Value].CapturedAt >= retention)
        {
            _errors.Remove(oldest.Value);
            _order.RemoveFirst();
        }
    }
}
