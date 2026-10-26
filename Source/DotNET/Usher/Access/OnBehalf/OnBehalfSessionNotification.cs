// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// The part every session notification from AuthProxy has: the started and the ended notification. Usher reads only
/// these from the body, and only to check that the body speaks of the session the token is for.
/// </summary>
/// <param name="SessionId">The session.</param>
/// <param name="Tenant">The tenant the session acts in.</param>
/// <param name="Issue">The issue the grant is for.</param>
/// <param name="Subject">The subject of the user the actor acts as.</param>
/// <param name="Actor">The actor.</param>
/// <param name="StartedAt">When the session started.</param>
public record OnBehalfSessionNotification(
    [property: JsonPropertyName("sessionId")] string SessionId,
    [property: JsonPropertyName("tenant")] string Tenant,
    [property: JsonPropertyName("issue")] string Issue,
    [property: JsonPropertyName("subject")] string Subject,
    [property: JsonPropertyName("actor")] OnBehalfActor Actor,
    [property: JsonPropertyName("startedAt")] DateTimeOffset StartedAt)
{
    /// <summary>
    /// Gets whether the notification speaks of the session a verified token is for.
    /// </summary>
    /// <param name="assertion">The <see cref="OnBehalfAssertion"/> of the token.</param>
    /// <returns>True when the session, tenant, issue, subject and actor are the token's.</returns>
    public bool IsAbout(OnBehalfAssertion assertion) =>
        SessionId == assertion.SessionId.Value &&
        Tenant == assertion.Tenant.Value &&
        Issue == assertion.Issue &&
        Subject == assertion.Subject.Value &&
        Actor?.UserId == assertion.Actor.Value;
}
