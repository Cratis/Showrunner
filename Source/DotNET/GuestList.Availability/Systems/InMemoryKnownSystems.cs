// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Contracts.Availability;

namespace Cratis.Showrunner.GuestList.Availability.Systems;

/// <summary>
/// Represents an <see cref="IKnownSystems"/> that holds the systems it was given. It knows none until it is given some,
/// which is how a product that has no systems reports.
/// </summary>
public class InMemoryKnownSystems : IKnownSystems
{
    volatile IReadOnlyCollection<SystemDeclaration> _current = [];

    /// <inheritdoc/>
    public IReadOnlyCollection<SystemDeclaration> Current => _current;

    /// <summary>
    /// Replaces the systems the product knows.
    /// </summary>
    /// <param name="systems">The systems.</param>
    public void Set(IEnumerable<SystemDeclaration> systems) => _current = [.. systems];
}
