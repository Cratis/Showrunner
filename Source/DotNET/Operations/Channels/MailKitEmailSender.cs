// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Cratis.Showrunner.Operations.Channels;

/// <summary>
/// Sends email with MailKit over SMTP with STARTTLS. Connect, login and send are bounded by the same timeout.
/// </summary>
/// <param name="options">The server and credentials.</param>
public sealed class MailKitEmailSender(SmtpOptions options) : IEmailSender
{
    const int TimeoutMilliseconds = 10_000;

    /// <inheritdoc/>
    public async Task Send(OutgoingEmail email, CancellationToken cancellationToken)
    {
        using var message = new MimeMessage();
        message.From.Add(new MailboxAddress(email.FromName, email.From));
        foreach (var recipient in email.To.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            message.To.Add(MailboxAddress.Parse(recipient));
        }

        message.Subject = email.Subject;
        message.Body = new TextPart("plain") { Text = email.Body };

        using var client = new SmtpClient { Timeout = TimeoutMilliseconds };
        await client.ConnectAsync(options.Server, options.Port, SecureSocketOptions.StartTls, cancellationToken);
        await client.AuthenticateAsync(options.Username, options.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
