// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Cratis.Showrunner.StageOperator.Reconciling;

/// <summary>Formats and parses the RFC 3339 timestamps of the resource.</summary>
public static class Rfc3339
{
    /// <summary>Formats a time in UTC.</summary>
    /// <param name="time">The time.</param>
    /// <returns>The timestamp.</returns>
    public static string Format(DateTimeOffset time) => time.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);

    /// <summary>Parses a timestamp.</summary>
    /// <param name="value">The timestamp, possibly empty.</param>
    /// <returns>The time, or null when the value is empty or not a timestamp.</returns>
    public static DateTimeOffset? Parse(string? value) =>
        DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var time) ? time : null;
}
