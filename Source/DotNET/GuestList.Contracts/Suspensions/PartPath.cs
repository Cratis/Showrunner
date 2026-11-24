// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Contracts.Suspensions;

/// <summary>
/// The path of a part of the application: a module, a feature, a slice, or one command, query or screen, as the
/// declaration report lists it, for example <c>Fleet/DataQuality</c>. The path covers itself and everything below it.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record PartPath(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// The longest a part path can be.
    /// </summary>
    public const int MaxLength = 512;

    /// <summary>
    /// Represents an unset <see cref="PartPath"/>.
    /// </summary>
    public static readonly PartPath NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="PartPath"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator PartPath(string value) => new(value);

    /// <summary>
    /// Gets whether a text is a valid part path: segments joined with <c>/</c>, none of them empty or holding white space.
    /// </summary>
    /// <param name="path">The text.</param>
    /// <returns><see langword="true"/> when it is; otherwise <see langword="false"/>.</returns>
    public static bool IsValidPath(string? path) =>
        path is { Length: > 0 and <= MaxLength } &&
        path.Split('/').All(segment => segment.Length > 0 && !segment.Any(char.IsWhiteSpace));
}
