// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Contracts.Audiences;

/// <summary>
/// The name of an audience, as the model declares it, for example <c>Plus</c>. A name starts with a letter and holds
/// letters, digits and underscores, up to 64 characters. Names are case-sensitive.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record AudienceName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// The longest an audience name can be.
    /// </summary>
    public const int MaxLength = 64;

    /// <summary>
    /// Represents an unset <see cref="AudienceName"/>.
    /// </summary>
    public static readonly AudienceName NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to an <see cref="AudienceName"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator AudienceName(string value) => new(value);

    /// <summary>
    /// Gets whether a text is a valid audience name.
    /// </summary>
    /// <param name="name">The text.</param>
    /// <returns><see langword="true"/> when it is; otherwise <see langword="false"/>.</returns>
    public static bool IsValidName(string? name) =>
        name is { Length: > 0 and <= MaxLength } &&
        char.IsAsciiLetter(name[0]) &&
        name.All(character => char.IsAsciiLetterOrDigit(character) || character == '_');
}
