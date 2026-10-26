// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Integration.given;

/// <summary>
/// An on-behalf session as AuthProxy holds it.
/// </summary>
/// <param name="SessionId">The opaque session identifier.</param>
/// <param name="Tenant">The tenant.</param>
/// <param name="Issue">The issue the grant is for.</param>
/// <param name="Subject">The subject of the user the actor acts as.</param>
/// <param name="Actor">The actor.</param>
/// <param name="StartedAt">When the session started.</param>
public record OnBehalfSession(string SessionId, string Tenant, string Issue, string Subject, ForwardedUser Actor, DateTimeOffset StartedAt);
