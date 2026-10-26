// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.Publishing.for_OutboxPublisher.given;

namespace Cratis.Showrunner.Usher.Publishing.for_OutboxPublisher.when_publishing;

public class a_lapsed_grant : an_outbox_publisher
{
    AccessExpired _published;

    async Task Because()
    {
        var context = ContextFor(2);
        await _publisher.GrantLapsed(new AccessGrantLapsed(Occurred), context, DeliveryOf(context));
        await ReadOutbox();
        _published = _outbox.Single().Content as AccessExpired;
    }

    [Fact] void should_publish_it_as_access_expired() => _published.ShouldNotBeNull();
    [Fact] void should_name_the_namespace_as_tenant() => _published.TenantId.ShouldEqual(new TenantId(Tenant.Value));
}
#endif
