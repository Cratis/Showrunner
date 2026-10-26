// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Integration.given;

/// <summary>
/// One call AuthProxy makes to the application.
/// </summary>
/// <param name="Method">The HTTP method.</param>
/// <param name="Target">The target.</param>
/// <param name="Body">The exact body, sent as UTF-8; <see langword="null"/> for the grant check.</param>
/// <param name="Token">The signed token.</param>
/// <param name="Tenant">The tenant of the session.</param>
/// <param name="ActorHeader">The actor, as AuthProxy forwards them.</param>
public record AuthProxyCall(HttpMethod Method, Uri Target, string? Body, string Token, string Tenant, string ActorHeader);
