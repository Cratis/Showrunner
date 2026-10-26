// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.Contracts.Identity;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// What a verified AuthProxy token says about an on-behalf session. Usher takes these facts from the token, never from
/// the query or the body of the call it came with.
/// </summary>
/// <param name="Subject">The subject of the user the actor acts as (<c>sub</c>).</param>
/// <param name="Actor">The subject of the actor (<c>act.sub</c>).</param>
/// <param name="ActorIdentityProvider">The identity provider of the actor (<c>act.idp</c>).</param>
/// <param name="Tenant">The tenant the session acts in (<c>tenant</c>).</param>
/// <param name="Issue">The issue the grant is for, as AuthProxy was given it (<c>issue</c>).</param>
/// <param name="SessionId">The on-behalf session (<c>sid</c>).</param>
public record OnBehalfAssertion(
    IdentityProviderSubject Subject,
    IdentityProviderSubject Actor,
    IdentityProviderName ActorIdentityProvider,
    TenantId Tenant,
    string Issue,
    SupportSessionId SessionId);
