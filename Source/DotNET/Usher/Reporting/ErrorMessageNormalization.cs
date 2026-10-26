// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;
using Cratis.Showrunner.Usher.Contracts.Reporting;

namespace Cratis.Showrunner.Usher.Reporting;

/// <summary>
/// Normalizes the message of an error so occurrences of the same error that differ only in ids and numbers read the
/// same, which is what lets them share a fingerprint.
/// </summary>
/// <remarks>
/// In order: GUIDs (with or without hyphens and braces) and hexadecimal ids (<c>0x</c> numbers, and runs of eight or
/// more hexadecimal characters that contain a digit) become <c>&lt;id&gt;</c>; every remaining run of digits, with
/// its decimal and thousands separators, becomes <c>&lt;n&gt;</c>; whitespace collapses to single spaces and the ends
/// are trimmed. Quoted values are kept: they usually name the field or entity the error is about, and ids inside them
/// are normalized by the rules above anyway. Casing is kept.
/// </remarks>
public static partial class ErrorMessageNormalization
{
    /// <summary>
    /// The placeholder an id is replaced with.
    /// </summary>
    public const string IdPlaceholder = "<id>";

    /// <summary>
    /// The placeholder a number is replaced with.
    /// </summary>
    public const string NumberPlaceholder = "<n>";

    [GeneratedRegex(@"\{?\b[0-9a-fA-F]{8}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{12}\b\}?", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Guids { get; }

    [GeneratedRegex(@"\b0[xX][0-9a-fA-F]+\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex HexadecimalNumbers { get; }

    [GeneratedRegex(@"\b(?=[0-9a-fA-F]*[0-9])[0-9a-fA-F]{8,}\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex HexadecimalIds { get; }

    [GeneratedRegex(@"\d+(?:[.,]\d+)*", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Numbers { get; }

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Whitespace { get; }

    /// <summary>
    /// Normalizes an error message.
    /// </summary>
    /// <param name="message">The <see cref="ErrorMessage"/> to normalize.</param>
    /// <returns>The normalized message; empty for an empty message.</returns>
    public static string Normalize(ErrorMessage message)
    {
        var normalized = message.Value ?? string.Empty;
        normalized = Guids.Replace(normalized, IdPlaceholder);
        normalized = HexadecimalNumbers.Replace(normalized, IdPlaceholder);
        normalized = HexadecimalIds.Replace(normalized, IdPlaceholder);
        normalized = Numbers.Replace(normalized, NumberPlaceholder);
        return Whitespace.Replace(normalized, " ").Trim();
    }
}
