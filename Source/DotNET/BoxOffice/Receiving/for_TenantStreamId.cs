// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Starting;

namespace Cratis.Showrunner.BoxOffice.Receiving;

public class for_TenantStreamId : Specification
{
    [Fact] void should_be_the_stream_the_tenant_entitlements_are_read_from() => TenantStreamId.Value.ShouldEqual(TenantStream.Id);
}
#endif
