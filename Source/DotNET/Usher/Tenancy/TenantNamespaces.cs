// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;

namespace Cratis.Showrunner.Usher.Tenancy;

/// <summary>
/// Finds the namespace of a tenant among the namespaces of the application's event store.
/// </summary>
/// <remarks>
/// A tenant is its namespace's exact name. A name that differs only in case is another tenant's, so matching is
/// ordinal: a desk naming <c>Fjord</c> never reaches the namespace <c>fjord</c>. More than one namespace with the exact
/// name means the namespaces can not be told apart, and none is chosen.
/// </remarks>
public static class TenantNamespaces
{
    /// <summary>
    /// Finds the namespace of a tenant.
    /// </summary>
    /// <param name="namespaces">The namespaces of the application's event store.</param>
    /// <param name="tenant">The tenant.</param>
    /// <returns>The <see cref="TenantNamespace"/> found, and whether it was found.</returns>
    public static TenantNamespace Find(IEnumerable<EventStoreNamespaceName> namespaces, TenantId tenant)
    {
        var matches = namespaces.Where(_ => string.Equals(_.Value, tenant.Value, StringComparison.Ordinal)).Take(2).ToArray();
        return matches.Length switch
        {
            1 => new(matches[0], TenantNamespaceMatch.Found),
            0 => new(null, TenantNamespaceMatch.Unknown),
            _ => new(null, TenantNamespaceMatch.Ambiguous)
        };
    }
}
