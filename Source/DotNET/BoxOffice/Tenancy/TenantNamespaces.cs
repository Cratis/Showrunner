// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;

namespace Cratis.Showrunner.BoxOffice.Tenancy;

/// <summary>
/// Finds the namespace of a tenant. A tenant's <see cref="TenantId"/> is the exact name of its Chronicle namespace.
/// </summary>
public static class TenantNamespaces
{
    /// <summary>
    /// Finds the namespace a tenant names.
    /// </summary>
    /// <param name="namespaces">The namespaces of the event store.</param>
    /// <param name="tenant">The tenant.</param>
    /// <returns>The match: a tenant never creates a namespace, and a name that two namespaces share chooses none.</returns>
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
