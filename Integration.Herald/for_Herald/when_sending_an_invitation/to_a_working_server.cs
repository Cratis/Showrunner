// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Herald.Integration.given;
using Cratis.Showrunner.Herald.Sending;
using MimeKit;

namespace Cratis.Showrunner.Herald.Integration.for_Herald.when_sending_an_invitation;

/// <summary>
/// An application sends a templated email with an image through SMTP to a test server, and the outcome is recorded.
/// </summary>
[Collection(ChronicleCollection.Name)]
public class to_a_working_server : Specification
{
    static readonly EventSourceId Invitation = "invitation-1";

    AnInvitationTemplate _files;
    CapturingSmtpServer _server;
    HeraldApplication _application;
    EmailId _id;
    EmailSent _sent;
    IReadOnlyList<object> _facts;
    MimeMessage _message;

    async Task Because()
    {
        _files = new();
        _server = new();
        _server.Start();
        _application = await HeraldApplication.Start(ChronicleInfrastructure.Current, _files.Templates, _files.Images, _server.Port);

        _id = await _application.Emails.Send(new EmailRequest(
            "invitation",
            Invitation,
            "Grace <grace@example.com>",
            new Dictionary<string, string>
            {
                ["invitation.link"] = "https://ledger.example.com/join?token=abc&x=1",
                ["inviter.name"] = "Ada",
                ["inviter.message"] = "<script>alert(1)</script>Welcome!"
            }));

        _sent = await _application.WaitFor<EmailSent>(Invitation, _id);
        await _application.WaitForEmptyQueue();
        _facts = await _application.Facts(Invitation);
        _message = _server.Received.Single();
    }

    async Task Destroy()
    {
        await _application.DisposeAsync();
        await _server.DisposeAsync();
        _files.Dispose();
    }

    [Fact] void should_record_that_it_was_sent_once() => Assert.Equal(1, _sent.Attempts);
    [Fact] void should_record_the_kind() => Assert.Equal("invitation", _sent.Kind.Value);
    [Fact] void should_deliver_to_the_recipient() => Assert.Equal("grace@example.com", _message.To.Mailboxes.Single().Address);
    [Fact] void should_deliver_from_the_sender() => Assert.Equal("noreply@ledger.example.com", _message.From.Mailboxes.Single().Address);
    [Fact] void should_fill_the_subject() => Assert.Equal("Ada invited you to Ledger", _message.Subject);
    [Fact] void should_deliver_the_image_embedded() => Assert.Single(_message.BodyParts.Where(part => part.ContentType.MimeType == "image/png"));
    [Fact] void should_refer_to_the_image_by_content_id() => Assert.Contains("cid:", _message.HtmlBody);
    [Fact] void should_deliver_a_plain_text_alternative() => Assert.Contains("Accept (https://ledger.example.com/join?token=abc&x=1)", _message.TextBody);
    [Fact] void should_encode_the_message_of_the_inviter() => Assert.DoesNotContain("<script>", _message.HtmlBody);
    [Fact] void should_have_taken_it_off_the_queue() => Assert.NotNull(_sent);
    [Fact] void should_record_exactly_a_queued_and_a_sent_fact() => Assert.Equal([typeof(EmailQueued), typeof(EmailSent)], _facts.Select(fact => fact.GetType()));
    [Fact] void should_never_keep_the_password_in_the_facts() => Assert.DoesNotContain(HeraldApplication.SmtpPassword, System.Text.Json.JsonSerializer.Serialize(_facts.Select(fact => (object)fact)));
}
