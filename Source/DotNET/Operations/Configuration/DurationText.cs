// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Cratis.Showrunner.Operations.Configuration;

/// <summary>
/// Reads durations written the way an operator writes them in an environment variable: <c>90s</c>, <c>30m</c>, <c>3h</c>,
/// <c>2d</c>, or a bare number of seconds.
/// </summary>
public static class DurationText
{
    /// <summary>
    /// Tries to read a duration.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="duration">The duration, when it could be read.</param>
    /// <returns><see langword="true"/> when the text is a positive duration.</returns>
    public static bool TryParse(string? text, out TimeSpan duration)
    {
        duration = default;
        var trimmed = text?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return false;
        }

        var unit = trimmed[^1];
        var number = char.IsDigit(unit) ? trimmed : trimmed[..^1];
        if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) || value <= 0)
        {
            return false;
        }

        duration = char.ToLowerInvariant(unit) switch
        {
            's' => TimeSpan.FromSeconds(value),
            'm' => TimeSpan.FromMinutes(value),
            'h' => TimeSpan.FromHours(value),
            'd' => TimeSpan.FromDays(value),
            _ when char.IsDigit(unit) => TimeSpan.FromSeconds(value),
            _ => TimeSpan.Zero
        };
        return duration > TimeSpan.Zero;
    }
}
