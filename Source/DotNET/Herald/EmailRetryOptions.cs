// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald;

/// <summary>
/// Represents how a failed delivery is retried.
/// </summary>
public class EmailRetryOptions
{
    /// <summary>
    /// Gets or sets how many deliveries are tried before the email fails for good. Defaults to 6.
    /// </summary>
    public int MaxAttempts { get; set; } = 6;

    /// <summary>
    /// Gets or sets the wait after the first failure; it doubles after each next one. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the longest wait between two deliveries. Defaults to one hour.
    /// </summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Gets the wait after a delivery failed.
    /// </summary>
    /// <param name="attempt">How many deliveries have failed, at least one.</param>
    /// <returns>The wait before the next delivery.</returns>
    public TimeSpan DelayAfter(int attempt)
    {
        var doublings = Math.Min(Math.Max(attempt - 1, 0), 30);
        var ticks = InitialDelay.Ticks * (1L << doublings);
        return ticks <= 0 || ticks > MaxDelay.Ticks ? MaxDelay : TimeSpan.FromTicks(ticks);
    }
}
