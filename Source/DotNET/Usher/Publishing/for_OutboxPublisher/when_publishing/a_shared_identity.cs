// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Identity;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;
using Cratis.Showrunner.Usher.Publishing.for_OutboxPublisher.given;

namespace Cratis.Showrunner.Usher.Publishing.for_OutboxPublisher.when_publishing;

public class a_shared_identity : an_outbox_publisher
{
    IssueAttributedToReporter _published;

    async Task Because()
    {
        var context = ContextFor(1);
        await _publisher.IdentityShared(new ReporterSharedIdentity(Reporters.Reporter), context, DeliveryOf(context));
        await ReadOutbox();
        _published = _outbox.Single().Content as IssueAttributedToReporter;
    }

    [Fact] void should_publish_it_as_issue_attributed_to_reporter() => _published.ShouldNotBeNull();
    [Fact] void should_carry_the_reporter() => _published.Reporter.ShouldEqual(Reporters.Reporter);
    [Fact] void should_name_the_namespace_as_tenant() => _published.TenantId.ShouldEqual(new TenantId(Tenant.Value));
}
#endif
