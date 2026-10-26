// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Compliance;
using Cratis.Chronicle.Compliance.GDPR;

namespace Cratis.Showrunner.Usher.Contracts.Identity;

/// <summary>
/// The display name of the reporter, as the identity provider gave it.
/// </summary>
/// <param name="Value">The underlying value.</param>
[PII]
[ComplianceDetails("Shown to the support desk so support can address the reporter, only after the reporter chose to share who they are.")]
public record ReporterName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ReporterName"/>, used when the identity provider gave no name.
    /// </summary>
    public static readonly ReporterName NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ReporterName"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ReporterName(string value) => new(value);
}
