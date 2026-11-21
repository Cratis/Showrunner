// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Alerts;

namespace Cratis.Showrunner.Operations.Channels;

/// <summary>
/// Delivers alerts by email.
/// </summary>
/// <param name="sender">The email sender.</param>
/// <param name="options">The addresses.</param>
public sealed class SmtpAlertChannel(IEmailSender sender, SmtpOptions options) : IAlertChannel
{
    /// <inheritdoc/>
    public string Name => "email";

    /// <inheritdoc/>
    public Task Deliver(AlertNotification notification, CancellationToken cancellationToken) =>
        sender.Send(new OutgoingEmail(options.FromName, options.From, options.To, notification.Subject, notification.Body), cancellationToken);
}
