// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Discovery;

/// <summary>
/// The event stores and namespaces a Production's Chronicle holds, as Chronicle reports them.
/// </summary>
/// <param name="EventStores">The event stores.</param>
public record ChronicleTopology(IReadOnlyList<EventStoreTopology> EventStores);
