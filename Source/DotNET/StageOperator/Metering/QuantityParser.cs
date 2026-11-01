// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Cratis.Showrunner.StageOperator.Metering;

/// <summary>Parses the Kubernetes quantities that metrics report, such as <c>250m</c>, <c>123456789n</c> or <c>512Mi</c>.</summary>
public static class QuantityParser
{
    static readonly (string Suffix, double Factor)[] _suffixes =
    [
        ("Ki", 1024d),
        ("Mi", 1024d * 1024),
        ("Gi", 1024d * 1024 * 1024),
        ("Ti", 1024d * 1024 * 1024 * 1024),
        ("n", 1e-9),
        ("u", 1e-6),
        ("m", 1e-3),
        ("k", 1e3),
        ("M", 1e6),
        ("G", 1e9),
        ("T", 1e12)
    ];

    /// <summary>Parses a quantity.</summary>
    /// <param name="value">The quantity.</param>
    /// <returns>The value in base units (cores or bytes), or zero when it cannot be parsed.</returns>
    public static double Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        foreach (var (suffix, factor) in _suffixes)
        {
            if (value.EndsWith(suffix, StringComparison.Ordinal) && double.TryParse(value[..^suffix.Length], NumberStyles.Float, CultureInfo.InvariantCulture, out var scaled))
            {
                return scaled * factor;
            }
        }

        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var plain) ? plain : 0;
    }
}
