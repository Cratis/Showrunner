// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;

namespace Cratis.Showrunner.Operations.Channels.for_SmtpAlertChannel;

public class when_delivering_an_alert : Specification
{
    IEmailSender _sender;
    OutgoingEmail _email;

    async Task Because()
    {
        _sender = Substitute.For<IEmailSender>();
        _sender.Send(Arg.Do<OutgoingEmail>(email => _email = email), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        var channel = new SmtpAlertChannel(_sender, new SmtpOptions("smtp.example.com", 587, "user", "pass", "Watchdog", "watchdog@example.com", "ops@example.com"));
        await channel.Deliver(new AlertNotification("shop", [new Finding("a", "a")], [new Finding("b", "b")], [], []), CancellationToken.None);
    }

    [Fact] void should_mail_the_recipient() => _email.To.ShouldEqual("ops@example.com");
    [Fact] void should_mail_from_the_configured_sender() => (_email.FromName, _email.From).ShouldEqual(("Watchdog", "watchdog@example.com"));
    [Fact] void should_use_the_subject_of_the_notification() => _email.Subject.ShouldEqual("ESCALATED: [shop] 2 cluster issue(s) need attention");
    [Fact] void should_use_the_body_of_the_notification() => _email.Body.ShouldContain("Escalated issues:");
    [Fact] void should_not_put_the_password_in_the_message() => _email.ToString().ShouldNotContain("pass");
}
#endif
