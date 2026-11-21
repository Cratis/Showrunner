// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_a_configuration_is_published : a_direct_link
{
    long _revision;
    DateTimeOffset _firstIssue;

    async Task Because()
    {
        _revision = PublishReadiness();
        await Pass();
        _firstIssue = Identity.Held!.Snapshot.IssuedAt;
        _time.Advance(TimeSpan.FromMinutes(31));
        await Pass();
    }

    [Fact] void should_apply_the_revision() => Status.AppliedRevision.ShouldEqual(_revision);
    [Fact] void should_acknowledge_the_applied_revision_with_its_digest() => (_direct.Acknowledged.Single().Revision, _direct.Acknowledged.Single().Digest).ShouldEqual((_revision, Identity.Held!.Snapshot.Digest));
    [Fact] void should_have_nothing_left_to_acknowledge() => Identity.AcknowledgeRevision.ShouldEqual(0);
    [Fact] void should_run_the_endpoint_check_exactly_as_pinned_with_its_credential() => _endpoints.Requests.ShouldContain(request => request.Uri.ToString() == "https://shop.internal/ready" && request.Authorization == "Bearer probe-secret");
    [Fact] void should_renew_the_lease_once_it_is_past_half() => Identity.Held!.Snapshot.IssuedAt.ShouldBeGreaterThan(_firstIssue);
    [Fact] void should_not_acknowledge_a_renewal_again() => _direct.Acknowledged.Count.ShouldEqual(1);
    [Fact] void should_keep_the_configuration_secrets_out_of_the_config_map() => _configMaps.Values.Values.Any(value => value.Contains("probe-secret") || value.Contains("admin-secret")).ShouldBeFalse();
}
#endif
