// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Alerts;

/// <summary>
/// What a delivery to every channel came to.
/// </summary>
/// <param name="Delivered">The names of the channels that delivered.</param>
/// <param name="Failed">The names of the channels that failed. No exception text: it can carry a capability URL.</param>
public sealed record DeliveryResult(IReadOnlyList<string> Delivered, IReadOnlyList<string> Failed)
{
    /// <summary>
    /// Gets a value indicating whether at least one channel delivered.
    /// </summary>
    public bool AnyDelivered => Delivered.Count > 0;
}
