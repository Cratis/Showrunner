// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Starting;

/// <summary>
/// The identity of the stream a tenant's facts are recorded in, within the tenant's namespace. It is the stream Box Office
/// reads a tenant's entitlements from (<c>TenantStream.Id</c>); Offering cannot reference Box Office, so it names it here.
/// </summary>
public static class TenantStreamId
{
    /// <summary>
    /// The event source id of the stream every tenant-wide fact is recorded in.
    /// </summary>
    public static readonly EventSourceId Value = new("tenant");
}
