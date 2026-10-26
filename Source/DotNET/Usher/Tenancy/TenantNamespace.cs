// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;

namespace Cratis.Showrunner.Usher.Tenancy;

/// <summary>
/// The namespace found for a tenant.
/// </summary>
/// <param name="Namespace">The namespace; <see langword="null"/> unless it was <see cref="TenantNamespaceMatch.Found"/>.</param>
/// <param name="Match">How the tenant matched the namespaces.</param>
public record TenantNamespace(EventStoreNamespaceName? Namespace, TenantNamespaceMatch Match);
