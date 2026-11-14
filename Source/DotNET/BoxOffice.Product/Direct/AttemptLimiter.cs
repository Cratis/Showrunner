// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// Limits how often the code can be tried, however many people or addresses try it.
/// </summary>
/// <param name="options">The <see cref="BoxOfficeProductOptions"/>.</param>
/// <param name="time">The <see cref="TimeProvider"/>.</param>
public class AttemptLimiter(IOptions<BoxOfficeProductOptions> options, TimeProvider time)
{
    readonly Lock _lock = new();
    readonly Queue<DateTimeOffset> _attempts = new();

    /// <summary>
    /// Counts an attempt, if the window has room for one.
    /// </summary>
    /// <returns><see langword="true"/> when the attempt may go on.</returns>
    public bool TryAcquire()
    {
        var settings = options.Value.Direct;
        var now = time.GetUtcNow();
        lock (_lock)
        {
            while (_attempts.Count > 0 && now - _attempts.Peek() >= settings.AttemptWindow)
            {
                _attempts.Dequeue();
            }

            if (_attempts.Count >= settings.MaximumAttempts)
            {
                return false;
            }

            _attempts.Enqueue(now);
            return true;
        }
    }
}
