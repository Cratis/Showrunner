// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice;

/// <summary>
/// The stream a tenant's facts are recorded in. A tenant has its own Chronicle namespace, so the namespace is what tells
/// tenants apart and the stream within it is the same for every tenant.
/// </summary>
public static class TenantStream
{
    /// <summary>
    /// The event source id of the stream every tenant-wide fact is recorded in, within the tenant's namespace.
    /// </summary>
    public static readonly EventSourceId Id = new("tenant");
}
