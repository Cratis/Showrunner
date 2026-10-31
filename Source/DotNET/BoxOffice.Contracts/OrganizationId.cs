// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts;

/// <summary>
/// Identifies one tenant organization. It is the event source id of every tenant-scoped Box Office event, in
/// both directions, and it never appears in an event payload. In Direct it is the organization's own identity.
/// As an event source id it is the lowercase, hyphenated <c>D</c> representation of the GUID.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record OrganizationId(Guid Value) : EventSourceId<Guid>(Value), IGeneratable<OrganizationId>
{
    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="OrganizationId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator OrganizationId(Guid value) => new(value);

    /// <summary>
    /// Creates a new <see cref="OrganizationId"/>.
    /// </summary>
    /// <returns>A new <see cref="OrganizationId"/>.</returns>
    public static OrganizationId New() => new(GenerateValue.Uuid());
}
