// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;

namespace Cratis.Showrunner.BoxOffice.Tenancy;

/// <summary>
/// The outcome of looking for the namespace of a tenant.
/// </summary>
/// <param name="Namespace">The namespace, when exactly one has the tenant's name.</param>
/// <param name="Match">How the tenant's name matched.</param>
public record TenantNamespace(EventStoreNamespaceName? Namespace, TenantNamespaceMatch Match);
