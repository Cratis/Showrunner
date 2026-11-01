// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle.Compliance;
using Cratis.Chronicle.Compliance.GDPR;

namespace Cratis.Showrunner.Herald.Sending;

/// <summary>
/// The values an email is filled with, kept as JSON so the event carries them whole. They can hold names and links, so
/// they are personal data.
/// </summary>
/// <param name="Value">The underlying value.</param>
[PII]
[ComplianceDetails("Fill the email with the names and links it is about, until the email was sent or failed.")]
public record EmailValues(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents no values.
    /// </summary>
    public static readonly EmailValues None = new("{}");

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to <see cref="EmailValues"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator EmailValues(string value) => new(value);

    /// <summary>
    /// Creates the values from a dictionary.
    /// </summary>
    /// <param name="values">The values by name.</param>
    /// <returns>The <see cref="EmailValues"/>.</returns>
    public static EmailValues From(IReadOnlyDictionary<string, string> values) => new(JsonSerializer.Serialize(values));

    /// <summary>
    /// Reads the values.
    /// </summary>
    /// <returns>The values by name.</returns>
    public IReadOnlyDictionary<string, string> ToDictionary() =>
        JsonSerializer.Deserialize<Dictionary<string, string>>(Value) ?? [];
}
