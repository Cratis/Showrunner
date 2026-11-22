// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Cratis.Showrunner.Playbill.Releases;

/// <summary>
/// The version a release is known by, such as <c>2.3.0</c> or <c>2.4.0-beta.1</c>.
/// </summary>
/// <remarks>
/// Versions are ordered the way semantic versioning orders them: by their numbers, with a prerelease before the release
/// it leads up to and build metadata ignored. A leading <c>v</c> is ignored. A value that is not a version sorts before
/// every version that is.
/// </remarks>
/// <param name="Value">The underlying value.</param>
public record ReleaseVersion(string Value) : ConceptAs<string>(Value), IComparable<ReleaseVersion>, IComparable
{
    /// <summary>
    /// Represents no version: nothing has been released, or nothing has been seen.
    /// </summary>
    public static readonly ReleaseVersion None = new(string.Empty);

    /// <summary>
    /// Gets a value indicating whether there is a version.
    /// </summary>
    public bool IsSet => !string.IsNullOrWhiteSpace(Value);

    /// <summary>
    /// Gets a value indicating whether the value is a version: one to three numbers, and optionally a prerelease.
    /// </summary>
    public bool IsValid => Parse(Value) is not null;

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ReleaseVersion"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ReleaseVersion(string value) => new(value);

    /// <summary>
    /// Gets whether a version is older than another.
    /// </summary>
    /// <param name="left">The version.</param>
    /// <param name="right">The version to compare with.</param>
    /// <returns><see langword="true"/> when <paramref name="left"/> is older; otherwise <see langword="false"/>.</returns>
    public static bool operator <(ReleaseVersion left, ReleaseVersion right) => Compare(left, right) < 0;

    /// <summary>
    /// Gets whether a version is newer than another.
    /// </summary>
    /// <param name="left">The version.</param>
    /// <param name="right">The version to compare with.</param>
    /// <returns><see langword="true"/> when <paramref name="left"/> is newer; otherwise <see langword="false"/>.</returns>
    public static bool operator >(ReleaseVersion left, ReleaseVersion right) => Compare(left, right) > 0;

    /// <summary>
    /// Gets whether a version is older than or the same as another.
    /// </summary>
    /// <param name="left">The version.</param>
    /// <param name="right">The version to compare with.</param>
    /// <returns><see langword="true"/> when <paramref name="left"/> is not newer; otherwise <see langword="false"/>.</returns>
    public static bool operator <=(ReleaseVersion left, ReleaseVersion right) => Compare(left, right) <= 0;

    /// <summary>
    /// Gets whether a version is newer than or the same as another.
    /// </summary>
    /// <param name="left">The version.</param>
    /// <param name="right">The version to compare with.</param>
    /// <returns><see langword="true"/> when <paramref name="left"/> is not older; otherwise <see langword="false"/>.</returns>
    public static bool operator >=(ReleaseVersion left, ReleaseVersion right) => Compare(left, right) >= 0;

    /// <summary>
    /// Orders two versions.
    /// </summary>
    /// <param name="left">The first version.</param>
    /// <param name="right">The second version.</param>
    /// <returns>A negative number when <paramref name="left"/> is older, a positive one when it is newer, and zero when they are the same.</returns>
    public static int Compare(ReleaseVersion? left, ReleaseVersion? right)
    {
        var first = Parse(left?.Value);
        var second = Parse(right?.Value);
        if (first is null || second is null)
        {
            if (first is null && second is null)
            {
                return string.CompareOrdinal(left?.Value ?? string.Empty, right?.Value ?? string.Empty);
            }

            return first is null ? -1 : 1;
        }

        for (var index = 0; index < 3; index++)
        {
            var difference = first.Value.Numbers[index].CompareTo(second.Value.Numbers[index]);
            if (difference != 0)
            {
                return Math.Sign(difference);
            }
        }

        return ComparePrerelease(first.Value.Prerelease, second.Value.Prerelease);
    }

    /// <inheritdoc/>
    public int CompareTo(ReleaseVersion? other) => Compare(this, other);

    /// <inheritdoc/>
    int IComparable.CompareTo(object? obj) => obj switch
    {
        null => 1,
        ReleaseVersion other => Compare(this, other),
        _ => throw new ArgumentException($"Cannot compare a {nameof(ReleaseVersion)} with a {obj.GetType().Name}.", nameof(obj))
    };

    static (long[] Numbers, string[] Prerelease)? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var text = value.Trim();
        if (text.StartsWith('v') || text.StartsWith('V'))
        {
            text = text[1..];
        }

        var plus = text.IndexOf('+');
        if (plus >= 0)
        {
            text = text[..plus];
        }

        var dash = text.IndexOf('-');
        var prerelease = dash >= 0 ? text[(dash + 1)..].Split('.') : [];
        var release = (dash >= 0 ? text[..dash] : text).Split('.');
        if (release.Length is < 1 or > 3)
        {
            return null;
        }

        var numbers = new long[3];
        for (var index = 0; index < release.Length; index++)
        {
            if (!long.TryParse(release[index], NumberStyles.None, CultureInfo.InvariantCulture, out numbers[index]))
            {
                return null;
            }
        }

        return (numbers, prerelease);
    }

    static int ComparePrerelease(string[] first, string[] second)
    {
        // A version with no prerelease is the release itself, which comes after everything that led up to it.
        if (first.Length == 0 || second.Length == 0)
        {
            if (first.Length == second.Length)
            {
                return 0;
            }

            return first.Length == 0 ? 1 : -1;
        }

        for (var index = 0; index < Math.Min(first.Length, second.Length); index++)
        {
            var firstNumeric = long.TryParse(first[index], NumberStyles.None, CultureInfo.InvariantCulture, out var firstNumber);
            var secondNumeric = long.TryParse(second[index], NumberStyles.None, CultureInfo.InvariantCulture, out var secondNumber);
            var difference = (firstNumeric, secondNumeric) switch
            {
                (true, true) => firstNumber.CompareTo(secondNumber),
                (true, false) => -1,
                (false, true) => 1,
                _ => string.CompareOrdinal(first[index], second[index])
            };

            if (difference != 0)
            {
                return Math.Sign(difference);
            }
        }

        return first.Length.CompareTo(second.Length);
    }
}
