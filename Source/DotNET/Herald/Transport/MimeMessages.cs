// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MimeKit;
using MimeKit.Utils;

namespace Cratis.Showrunner.Herald.Transport;

/// <summary>
/// Builds the MIME message for an <see cref="EmailMessage"/>: plain text and HTML as alternatives, with the images
/// embedded as related parts the HTML refers to by content id.
/// </summary>
public static class MimeMessages
{
    /// <summary>
    /// Creates the MIME message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The MIME message.</returns>
    public static MimeMessage From(EmailMessage message)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(message.From.Name, message.From.Address));
        mime.To.Add(new MailboxAddress(message.To.Name, message.To.Address));
        if (message.ReplyTo is not null)
        {
            mime.ReplyTo.Add(new MailboxAddress(message.ReplyTo.Name, message.ReplyTo.Address));
        }

        mime.Subject = message.Subject;

        var html = message.Html;
        var builder = new BodyBuilder { TextBody = message.Text };
        foreach (var image in message.Images)
        {
            var part = builder.LinkedResources.Add(image.Name, image.Content.ToArray(), ContentType.Parse(image.MediaType));
            part.ContentId = MimeUtils.GenerateMessageId();
            html = html.Replace($"cid:{image.Name}", $"cid:{part.ContentId}", StringComparison.Ordinal);
        }

        builder.HtmlBody = html;
        mime.Body = builder.ToMessageBody();
        return mime;
    }
}
