// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Transport;

/// <summary>
/// The identity of an event type on the wire.
/// </summary>
/// <param name="Id">The event type id.</param>
/// <param name="Generation">The generation of the event type.</param>
public record PublishedEventType(string Id, uint Generation);
