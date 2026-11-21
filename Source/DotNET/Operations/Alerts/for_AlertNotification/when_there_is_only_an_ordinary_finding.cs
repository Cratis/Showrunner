// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Alerts.for_AlertNotification;

public class when_there_is_only_an_ordinary_finding : Specification
{
    AlertNotification _notification;

    void Establish() => _notification = new AlertNotification("shop", [new Finding("a", "new a", AlertSeverity.Warning)], [], [], []);

    [Fact] void should_not_carry_the_escalation_prefix() => _notification.Subject.ShouldEqual("[shop] 1 cluster issue(s) need attention");
    [Fact] void should_start_with_what_is_new() => _notification.Body.ShouldContain("New since the last alert:\n- new a");
    [Fact] void should_not_mention_escalation() => _notification.Body.ShouldNotContain("Escalated");
    [Fact] void should_not_mention_ongoing_issues() => _notification.Body.ShouldNotContain("Still ongoing");
    [Fact] void should_take_the_severity_of_the_finding() => _notification.Severity.ShouldEqual(AlertSeverity.Warning);
}
#endif
