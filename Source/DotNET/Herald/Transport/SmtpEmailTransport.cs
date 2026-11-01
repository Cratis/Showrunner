// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Herald.Transport;

/// <summary>
/// Sends messages to a mail server over SMTP with MailKit, one connection per message.
/// </summary>
/// <param name="options">The <see cref="HeraldOptions"/>.</param>
public class SmtpEmailTransport(IOptions<HeraldOptions> options) : IEmailTransport
{
    /// <inheritdoc/>
    public async Task Send(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var smtp = options.Value.Smtp;
        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(smtp.Host, smtp.Port, SecurityFor(smtp.Security), cancellationToken);
            if (smtp.UserName.Length > 0)
            {
                await client.AuthenticateAsync(smtp.UserName, smtp.Password, cancellationToken);
            }

            using var mime = MimeMessages.From(message);
            await client.SendAsync(mime, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (SmtpCommandException exception) when (IsRefusalOfTheMessage(exception))
        {
            throw new EmailTransportException($"The mail server refused the message: {(int)exception.StatusCode} {exception.ErrorCode}.", permanent: true, exception);
        }
        catch (Exception exception) when (exception is ServiceNotConnectedException or ServiceNotAuthenticatedException or SmtpCommandException or SmtpProtocolException or AuthenticationException or IOException or System.Net.Sockets.SocketException or TimeoutException or SslHandshakeException)
        {
            throw new EmailTransportException($"The mail server at {smtp} could not take the message: {exception.GetType().Name}.", permanent: false, exception);
        }
    }

    static bool IsRefusalOfTheMessage(SmtpCommandException exception) =>
        exception.ErrorCode is SmtpErrorCode.RecipientNotAccepted or SmtpErrorCode.SenderNotAccepted or SmtpErrorCode.MessageNotAccepted
        && (int)exception.StatusCode is >= 550 and <= 554;

    static SecureSocketOptions SecurityFor(SmtpSecurity security) => security switch
    {
        SmtpSecurity.Tls => SecureSocketOptions.SslOnConnect,
        SmtpSecurity.None => SecureSocketOptions.None,
        _ => SecureSocketOptions.StartTls
    };
}
