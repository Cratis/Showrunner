// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Conversation.AddDetails;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation;

public class when_events_precede_a_report : a_read_model<IssueConversation>
{
    async Task Because() => await Happened(new ReporterAddedDetails("More details"), new AccessGrantLapsed(Start), new SupportSignedInAsReporter(Reporters.SessionId, IssueEvents.Support, IssueEvents.SupportPerson), new SupportSessionEnded(Reporters.SessionId), new SupportAskedQuestion(QuestionId.New(), "Which invoice?", IssueEvents.Support));

    [Fact] void should_not_invent_a_conversation() => _scenario.Instance.ShouldBeNull();
}
#endif
