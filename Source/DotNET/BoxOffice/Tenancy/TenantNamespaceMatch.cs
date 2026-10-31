// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Tenancy;

/// <summary>
/// How a tenant's name matched the namespaces of the event store.
/// </summary>
public enum TenantNamespaceMatch
{
    /// <summary>
    /// Exactly one namespace has the tenant's name.
    /// </summary>
    Found = 0,

    /// <summary>
    /// No namespace has the tenant's exact name.
    /// </summary>
    Unknown = 1,

    /// <summary>
    /// More than one namespace has the tenant's name, so none is chosen.
    /// </summary>
    Ambiguous = 2
}
