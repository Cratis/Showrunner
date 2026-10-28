// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;

namespace Cratis.Showrunner.Usher.Access.GrantAccess.when_granting_access;

public class and_identity_is_already_shared : an_issue_command<GrantAccess>
{
    void Establish() => GivenReported(new ReporterSharedIdentity(Reporters.Reporter));

    async Task Because() => _result = await _scenario.Execute(new GrantAccess(_issueId, AccessDuration.OneDay));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
    [Fact] void should_only_grant_access() =>
        _scenario.AppendedEvents.Select(_ => _.Event.Content.GetType()).ShouldEqual([typeof(ReporterGrantedAccess)]);
}
#endif
