// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Compliance;
using Cratis.Chronicle.Compliance.GDPR;

namespace Cratis.Showrunner.Usher.Contracts.Lifecycle;

/// <summary>
/// Why the reporter reopened an issue support had resolved.
/// </summary>
/// <param name="Value">The underlying value.</param>
[PII]
[ComplianceDetails("Free text the reporter writes when reopening an issue; it can name or identify them and is shown to support to work on the issue.")]
public record ReopenReason(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ReopenReason"/>.
    /// </summary>
    public static readonly ReopenReason NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ReopenReason"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ReopenReason(string value) => new(value);
}
