// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Transport;

/// <summary>
/// The users a desk currently lets give feedback in the tenant of a publication, as the collector hands it to an application
/// that runs its own kernel. It names each user by their <see cref="Access.FeedbackGiverId"/>, which the application derives
/// from the identity of the signed-in user, so no identity provider or subject leaves the collector.
/// </summary>
/// <param name="Version">What the list is as of: one more than the highest sequence number of the desk's decisions the collector holds for the tenant, zero when it holds none. It changes whenever a decision is recorded, and it is also the entity tag of the answer.</param>
/// <param name="Givers">The users who are granted the right now; a user who was revoked, or never decided about, is not in the list.</param>
public record FeedbackAccessGrants(ulong Version, IReadOnlyList<Guid> Givers);
