// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Herald.Rendering;
using Cratis.Showrunner.Herald.Transport;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Herald.Sending;

/// <summary>
/// Renders and sends a queued email, describing dispatch failures without exposing recipient or server secrets.
/// </summary>
/// <param name="renderer">The renderer.</param>
/// <param name="transport">The transport.</param>
/// <param name="reasons">The safe failure descriptions.</param>
/// <param name="options">The sender options.</param>
sealed class EmailDispatch(EmailRenderer renderer, IEmailTransport transport, FailureReasons reasons, IOptions<HeraldOptions> options)
{
    /// <summary>
    /// Renders the queued email in its requested language.
    /// </summary>
    /// <param name="queued">The queued email.</param>
    /// <returns>The rendered email.</returns>
    internal RenderedEmail Render(EmailQueued queued) =>
        renderer.Render(queued.Kind, queued.Language.Value.Length == 0 ? null : queued.Language, queued.Values.ToDictionary());

    /// <summary>
    /// Sends the rendered email with the configured sender.
    /// </summary>
    /// <param name="queued">The queued email.</param>
    /// <param name="rendered">The rendered email.</param>
    /// <param name="cancellationToken">Cancels the send.</param>
    /// <returns>An awaitable task.</returns>
    internal Task Send(EmailQueued queued, RenderedEmail rendered, CancellationToken cancellationToken) =>
        transport.Send(MessageFor(queued, rendered), cancellationToken);

    /// <summary>
    /// Describes a dispatch failure without the recipient's address or server secrets.
    /// </summary>
    /// <param name="exception">The failure.</param>
    /// <param name="queued">The queued email.</param>
    /// <returns>The safe failure reason.</returns>
    internal FailureReason ReasonFor(Exception exception, EmailQueued queued) => reasons.For(exception, queued.Address.Value);

    EmailMessage MessageFor(EmailQueued queued, RenderedEmail rendered)
    {
        var sender = options.Value.Sender;
        return new(
            new(sender.Address, sender.Name),
            new(queued.Address.Value, queued.Name.Value),
            sender.ReplyTo.Length == 0 ? null : new(sender.ReplyTo),
            rendered.Subject,
            rendered.Html,
            rendered.Text,
            rendered.Images);
    }
}
