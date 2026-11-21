// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// The instance stopped trying a remedy - the budget is spent or it did not verify - and a person must act.
/// </summary>
/// <param name="Remedy">The remedy's name.</param>
/// <param name="Resource">The resource.</param>
/// <param name="Attempts">How many attempts were made.</param>
/// <param name="Reason">Why it escalated.</param>
public record RemedyEscalated(string Remedy, Guid Resource, int Attempts, string Reason);
