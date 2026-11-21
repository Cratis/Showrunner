// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// A remedy was started.
/// </summary>
/// <param name="Remedy">The remedy's name.</param>
/// <param name="Resource">The resource it acts on.</param>
/// <param name="Attempt">The attempt number within the budget window.</param>
/// <param name="Offline">Whether it ran under the offline policy rather than a Direct authorization.</param>
/// <param name="Authorization">The Direct remedy request that authorized it - empty offline.</param>
/// <param name="ConfigurationRevision">The revision it ran under.</param>
/// <param name="FencingEpoch">The epoch the instance held.</param>
public record RemedyAttempted(string Remedy, Guid Resource, int Attempt, bool Offline, Guid Authorization, long ConfigurationRevision, long FencingEpoch);
