// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Herald.Integration.given;
using Cratis.Showrunner.Herald.Sending;

namespace Cratis.Showrunner.Herald.Integration.for_Herald.when_sending_an_invitation;

/// <summary>
/// The mail server is down when the email is sent: it stays queued, is retried, and goes out once the server is back.
/// </summary>
[Collection(ChronicleCollection.Name)]
public class to_a_server_that_is_down : Specification
{
    static readonly EventSourceId Invitation = "invitation-2";

    AnInvitationTemplate _files;
    CapturingSmtpServer _server;
    HeraldApplication _application;
    EmailId _id;
    EmailAttemptFailed _failedAttempt;
    EmailSent _sent;
    IReadOnlyList<object> _facts;

    async Task Because()
    {
        _files = new();
        _server = new();
        _application = await HeraldApplication.Start(ChronicleInfrastructure.Current, _files.Templates, _files.Images, _server.Port);

        _id = await _application.Emails.Send(new EmailRequest(
            "invitation",
            Invitation,
            "grace@example.com",
            new Dictionary<string, string> { ["invitation.link"] = "https://ledger.example.com/join", ["inviter.name"] = "Ada" }));

        _failedAttempt = await _application.WaitFor<EmailAttemptFailed>(Invitation, _id);
        _server.Start();
        _sent = await _application.WaitFor<EmailSent>(Invitation, _id);
        _facts = await _application.Facts(Invitation);
    }

    async Task Destroy()
    {
        await _application.DisposeAsync();
        await _server.DisposeAsync();
        _files.Dispose();
    }

    [Fact] void should_record_the_failed_attempt() => Assert.Equal(1, _failedAttempt.Attempt);
    [Fact] void should_record_why_without_the_password() => Assert.DoesNotContain(HeraldApplication.SmtpPassword, _failedAttempt.Reason.Value);
    [Fact] void should_send_it_when_the_server_is_back() => Assert.True(_sent.Attempts >= 2);
    [Fact] void should_deliver_it_once() => Assert.Single(_server.Received);
    [Fact] void should_not_record_a_failure_for_good() => Assert.DoesNotContain(_facts, fact => fact is EmailFailed);
}
