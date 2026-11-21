// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// A batch of outbox entries, in ascending sequence. Send one batch at a time and wait for its receipt.
/// </summary>
/// <param name="Events">The entries.</param>
public record WatchdogEventBatch(IReadOnlyList<WatchdogEventEnvelope> Events);
