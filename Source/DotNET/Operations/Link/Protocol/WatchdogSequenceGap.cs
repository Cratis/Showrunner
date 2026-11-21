// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// A span of outbox sequences Direct never received.
/// </summary>
/// <param name="From">The first missing sequence.</param>
/// <param name="To">The last missing sequence.</param>
public record WatchdogSequenceGap(long From, long To);
