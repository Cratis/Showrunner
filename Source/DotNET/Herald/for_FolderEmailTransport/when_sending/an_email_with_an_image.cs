// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Transport;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Cratis.Showrunner.Herald.for_FolderEmailTransport.when_sending;

public class an_email_with_an_image : given.a_templates_folder
{
    MimeMessage _message;
    string _file;

    async Task Because()
    {
        var rendered = _renderer.Render("invitation", null, InvitationValues);
        var transport = new FolderEmailTransport(Options.Create(_options));
        await transport.Send(
            new(new("noreply@ledger.example.com", "Ledger"), new("grace@example.com", "Grace"), new("help@ledger.example.com"), rendered.Subject, rendered.Html, rendered.Text, rendered.Images));
        _file = Directory.GetFiles(_options.PickupFolder, "*.eml").Single();
        _message = await MimeMessage.LoadAsync(_file);
    }

    [Fact] void should_write_one_eml_file() => Assert.EndsWith(".eml", _file);
    [Fact] void should_address_the_recipient() => Assert.Equal("grace@example.com", _message.To.Mailboxes.Single().Address);
    [Fact] void should_set_the_sender() => Assert.Equal("Ledger", _message.From.Mailboxes.Single().Name);
    [Fact] void should_set_where_replies_go() => Assert.Equal("help@ledger.example.com", _message.ReplyTo.Mailboxes.Single().Address);
    [Fact] void should_carry_the_plain_text_alternative() => Assert.Contains("Accept (", _message.TextBody);
    [Fact] void should_carry_the_html_alternative() => Assert.Contains("<a ", _message.HtmlBody);
    [Fact] void should_embed_the_image() => Assert.Single(_message.BodyParts, part => part.ContentType.MimeType == "image/png");
    [Fact] void should_refer_to_the_embedded_image_by_content_id() => Assert.Contains($"cid:{_message.BodyParts.First(part => part.ContentType.MimeType == "image/png").ContentId}", _message.HtmlBody);
}
#endif
