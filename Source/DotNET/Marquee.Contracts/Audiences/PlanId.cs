// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Contracts.Audiences;

/// <summary>
/// Names a plan of a product, for an announcement aimed at the tenants on it. Marquee only carries the id; whether a
/// tenant is on a plan is answered by an <see cref="IAudienceEntitlements"/> the host plugs in.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record PlanId(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="PlanId"/>.
    /// </summary>
    public static readonly PlanId NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="PlanId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator PlanId(string value) => new(value);
}
