// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Choreography.Availability.Audiences;
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Holding;

namespace Cratis.Showrunner.GuestList.Availability;

/// <summary>
/// Translates a change to an <see cref="AudienceHoldings"/> read model into the <see cref="AudienceChange"/> the engine
/// understands.
/// </summary>
public static class AudienceChanges
{
    const string UserPrefix = "user:";

    /// <summary>
    /// Gets the change a read model instance changing means.
    /// </summary>
    /// <param name="namespace">The namespace the read model changed in: a tenant's, or the product's own for the system.</param>
    /// <param name="holdingsId">The stream of the read model: see <see cref="GuestListStreams"/>.</param>
    /// <returns>The change, or <see langword="null"/> when the stream is not one of GuestList's.</returns>
    public static AudienceChange? From(EventStoreNamespaceName @namespace, string holdingsId)
    {
        if (holdingsId == GuestListStreams.System.Value ||
            (holdingsId.StartsWith(GuestListStreams.NamedSystemPrefix, StringComparison.Ordinal) && holdingsId.Length > GuestListStreams.NamedSystemPrefix.Length))
        {
            return new(AudienceLevel.System, null);
        }

        if (holdingsId == GuestListStreams.Tenant.Value)
        {
            return new(AudienceLevel.Tenant, @namespace.Value);
        }

        return holdingsId.StartsWith(UserPrefix, StringComparison.Ordinal) && holdingsId.Length > UserPrefix.Length
            ? new(AudienceLevel.User, holdingsId[UserPrefix.Length..])
            : null;
    }
}
