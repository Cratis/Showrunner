// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Arc.Chronicle.Testing.Commands;

namespace Cratis.Showrunner.Notes.Access.for_FeedbackGiver;

/// <summary>
/// The command decides on <see cref="FeedbackGiver"/> with a read that guards the append, which Chronicle admits only for a
/// model of a shape it can fold from the user's events: this checks the shape without running a command.
/// </summary>
public class when_admitting_decision_reads : Specification
{
    [Fact] void should_admit_the_decision_read_of_the_state() => DecisionReadAdmissions.ShouldAdmitDecisionReadsOf(typeof(FeedbackGiver));
}
#endif
