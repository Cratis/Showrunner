// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Compliance;
using Cratis.Chronicle.Compliance.GDPR;

namespace Cratis.Showrunner.BoxOffice.Contracts.People;

/// <summary>
/// A person's email address.
/// </summary>
/// <param name="Value">The underlying value.</param>
[PII]
[ComplianceDetails("The email address used by the operator and tenant administrators to contact a seat holder or the person who registered the tenant.")]
public record Email(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="Email"/>.
    /// </summary>
    public static readonly Email NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="Email"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator Email(string value) => new(value);
}
