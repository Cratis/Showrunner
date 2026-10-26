// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Compliance;
using Cratis.Chronicle.Compliance.GDPR;

namespace Cratis.Showrunner.Usher.Contracts.Reporting;

/// <summary>
/// The reporter's one-line summary of an issue they describe by hand; it becomes the issue's title.
/// </summary>
/// <param name="Value">The underlying value.</param>
[PII]
[ComplianceDetails("Free text the reporter writes to describe their problem; it can name or identify them and is shown to support to work on the issue.")]
public record Summary(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="Summary"/>.
    /// </summary>
    public static readonly Summary NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="Summary"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator Summary(string value) => new(value);
}
