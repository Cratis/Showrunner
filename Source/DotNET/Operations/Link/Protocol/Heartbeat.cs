// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// The instance is alive - folded into its status by Direct, never recorded as an event.
/// </summary>
/// <param name="AppliedRevision">The configuration revision it runs.</param>
/// <param name="RuntimeVersion">Its runtime version.</param>
/// <param name="DurableQueue">Whether its outbox is on durable storage.</param>
/// <param name="QueueDepth">How many outbox entries are not acknowledged.</param>
public record Heartbeat(long AppliedRevision, string RuntimeVersion, bool DurableQueue, int QueueDepth);
