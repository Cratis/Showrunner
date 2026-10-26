// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation;

public class when_support_requests_access_and_it_is_granted : a_read_model<IssueConversation>
{
    static readonly ValidUntil _validUntil = new(Start.AddDays(1));
    IssueAccess _requested;
    IssueConversation _issue;

    async Task Because()
    {
        await Happened(IssueEvents.Described(Reporters.Subject), new SupportRequestedAccess("To see the invoice."));
        _requested = _scenario.Instance!.Access;
        await Happened(new ReporterSharedIdentity(Reporters.Reporter), new ReporterGrantedAccess(_validUntil));
        _issue = _scenario.Instance!;
    }

    [Fact] void should_have_the_request_pending() => _requested.Request.ShouldEqual(new AccessRequest(true, "To see the invoice."));
    [Fact] void should_not_be_granted_while_requested() => _requested.State.ShouldEqual(AccessState.NotGranted);
    [Fact] void should_be_granted() => _issue.Access.State.ShouldEqual(AccessState.Granted);
    [Fact] void should_have_the_end_of_the_grant() => _issue.Access.ValidUntil.ShouldEqual(_validUntil);
    [Fact] void should_answer_the_request() => _issue.Access.Request.ShouldEqual(AccessRequest.None);
    [Fact] void should_have_identity_shared() => _issue.IdentityShared.ShouldBeTrue();

    [Fact]
    void should_record_the_request_the_identity_and_the_grant() => _issue.Entries.Skip(1).Select(_ => (_.Kind, _.AuthorKind)).ShouldEqual(
    [
        (ConversationEntryKind.AccessRequested, ConversationAuthorKind.System),
        (ConversationEntryKind.IdentityShared, ConversationAuthorKind.Reporter),
        (ConversationEntryKind.AccessGranted, ConversationAuthorKind.Reporter)
    ]);
}
#endif
