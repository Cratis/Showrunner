// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Tenants;

/// <summary>
/// Why a product refused to create a tenant, in words the operator can show to a person.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record NotProvisionedReason(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="NotProvisionedReason"/>.
    /// </summary>
    public static readonly NotProvisionedReason NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="NotProvisionedReason"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator NotProvisionedReason(string value) => new(value);
}
