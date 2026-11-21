// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Alerts.for_AlertNotification;

public class when_there_are_new_escalated_and_ongoing_findings : Specification
{
    AlertNotification _notification;

    void Establish() => _notification = new AlertNotification(
        "shop",
        [new Finding("b-new", "new b", AlertSeverity.Warning), new Finding("a-new", "new a", AlertSeverity.Warning)],
        [new Finding("e", "escalated e", AlertSeverity.Critical)],
        [new Finding("o", "ongoing o")],
        []);

    [Fact] void should_count_every_open_issue() => _notification.Subject.ShouldEqual("ESCALATED: [shop] 4 cluster issue(s) need attention");
    [Fact] void should_put_the_escalated_issues_first() => _notification.Body.ShouldContain("Escalated issues:\n- escalated e\n\nNew since the last alert:");
    [Fact] void should_sort_the_new_issues_by_key() => _notification.Body.ShouldContain("- new a\n- new b");
    [Fact] void should_list_what_is_still_ongoing() => _notification.Body.ShouldContain("Still ongoing (alerted earlier):\n- ongoing o");
    [Fact] void should_take_the_highest_severity() => _notification.Severity.ShouldEqual(AlertSeverity.Critical);
    [Fact] void should_name_the_issues_that_need_a_human_now() => _notification.Actionable.Select(finding => finding.Key).ShouldContainOnly("b-new", "a-new", "e");
}
#endif
