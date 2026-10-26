// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation;

public class when_access_is_revoked : a_read_model<IssueConversation>
{
    IssueConversation _issue;

    async Task Because()
    {
        await Happened(IssueEvents.Described(Reporters.Subject), new ReporterGrantedAccess(Start.AddHours(1)), new ReporterRevokedAccess());
        _issue = _scenario.Instance!;
    }

    [Fact] void should_be_revoked() => _issue.Access.State.ShouldEqual(AccessState.Revoked);
    [Fact] void should_record_the_revoke() => _issue.Entries.Last().Kind.ShouldEqual(ConversationEntryKind.AccessRevoked);
}
#endif
