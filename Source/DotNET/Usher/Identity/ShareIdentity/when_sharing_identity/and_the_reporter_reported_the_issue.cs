// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Identity.ShareIdentity.when_sharing_identity;

public class and_the_reporter_reported_the_issue : an_issue_command<ShareIdentity>
{
    void Establish() => GivenReported();

    async Task Because() => _result = await _scenario.Execute(new ShareIdentity(_issueId));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_share_who_the_signed_in_reporter_is() =>
        await _scenario.ShouldHaveAppendedEvent<ShareIdentity, ReporterSharedIdentity>(_issueId, e => e.Reporter == Reporters.Reporter);
}
#endif
