// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.SignIns;

/// <summary>
/// The headers AuthProxy sends with its sign-in callback.
/// </summary>
public static class SignInHeaders
{
    /// <summary>
    /// The header that marks a call as AuthProxy's sign-in callback; its value is the kind.
    /// </summary>
    public const string Marker = "x-cratis-sign-in-callback";

    /// <summary>
    /// The header that carries the shared secret, when one is configured.
    /// </summary>
    public const string Secret = "x-cratis-sign-in-callback-secret";

    /// <summary>
    /// The header that carries the subject of the person, as on every proxied request.
    /// </summary>
    public const string PrincipalId = "x-ms-client-principal-id";

    /// <summary>
    /// The header that carries the tenant AuthProxy resolved.
    /// </summary>
    public const string Tenant = "x-cratis-tenant-id";

    /// <summary>
    /// The earlier name of the header that carries the tenant.
    /// </summary>
    public const string LegacyTenant = "Tenant-ID";
}
