// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Discovery;

/// <summary>
/// One event store and its namespaces.
/// </summary>
/// <param name="Name">The name of the event store.</param>
/// <param name="Namespaces">The names of its namespaces.</param>
public record EventStoreTopology(string Name, IReadOnlyList<string> Namespaces);
