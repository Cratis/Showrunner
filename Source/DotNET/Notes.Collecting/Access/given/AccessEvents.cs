// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Access;
using Cratis.Showrunner.Notes.Contracts.Access;
using Cratis.Showrunner.Usher.Contracts.Identity;

namespace Cratis.Showrunner.Notes.Collecting.Access.given;

/// <summary>
/// What the desk decides about who may give feedback, as the specifications use it.
/// </summary>
public static class AccessEvents
{
    /// <summary>
    /// The person at the desk who decides.
    /// </summary>
    public static readonly FeedbackGrantor Grantor = "desk-operator-1";

    /// <summary>
    /// Someone else at the desk.
    /// </summary>
    public static readonly FeedbackGrantor AnotherGrantor = "desk-operator-2";

    /// <summary>
    /// The tenant namespace the collecting store serves.
    /// </summary>
    public static readonly TenantId Tenant = "fjord-logistics";

    /// <summary>
    /// A user of the collecting store, granted by the specifications.
    /// </summary>
    public static readonly FeedbackGiverIdentity Giver = new("github", "1234");

    /// <summary>
    /// The private fact that the desk granted the right.
    /// </summary>
    /// <returns>The fact.</returns>
    public static FeedbackAccessGrantReceived Granted() => new(Grantor);

    /// <summary>
    /// The private fact that the desk revoked the right.
    /// </summary>
    /// <returns>The fact.</returns>
    public static FeedbackAccessRevocationReceived Revoked() => new(Grantor);

    /// <summary>
    /// The public event that the desk granted the right to the specifications' user.
    /// </summary>
    /// <returns>The event.</returns>
    public static FeedbackAccessGranted PublicGranted() => new(Tenant, Giver, Grantor);

    /// <summary>
    /// The public event that the desk revoked the right from the specifications' user.
    /// </summary>
    /// <returns>The event.</returns>
    public static FeedbackAccessRevoked PublicRevoked() => new(Tenant, Giver, Grantor);
}
#endif
