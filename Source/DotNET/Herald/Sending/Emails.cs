// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Showrunner.Herald.Rendering;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Cratis.Showrunner.Herald.Sending;

/// <summary>
/// The application's way of sending email: <see cref="IEmails"/>.
/// </summary>
/// <param name="client">The <see cref="IChronicleClient"/>.</param>
/// <param name="chronicle">The <see cref="ChronicleClientOptions"/>, which name the event store when nothing else does.</param>
/// <param name="options">The <see cref="HeraldOptions"/>.</param>
/// <param name="renderer">The <see cref="EmailRenderer"/>.</param>
/// <param name="time">The <see cref="TimeProvider"/>.</param>
public class Emails(
    IChronicleClient client,
    IOptions<ChronicleClientOptions> chronicle,
    IOptions<HeraldOptions> options,
    EmailRenderer renderer,
    TimeProvider time) : IEmails
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Emails"/> class.
    /// </summary>
    /// <param name="client">The <see cref="IChronicleClient"/>.</param>
    /// <param name="chronicle">The <see cref="ChronicleClientOptions"/>.</param>
    /// <param name="options">The <see cref="HeraldOptions"/>.</param>
    /// <param name="renderer">The <see cref="EmailRenderer"/>.</param>
    /// <param name="time">The <see cref="TimeProvider"/>.</param>
    /// <param name="eventStores">The <see cref="HeraldEventStore"/> naming the application's event store.</param>
    public Emails(
        IChronicleClient client,
        IOptions<ChronicleClientOptions> chronicle,
        IOptions<HeraldOptions> options,
        EmailRenderer renderer,
        TimeProvider time,
        HeraldEventStore eventStores)
        : this(client, chronicle, options, renderer, time) => EventStores = eventStores;

    HeraldEventStore EventStores { get => field ??= HeraldEventStore.ForClientOptions(chronicle, options); init; }

    /// <inheritdoc/>
    public async Task<EmailId> Send(EmailRequest request, CancellationToken cancellationToken = default)
    {
        if (!MailboxAddress.TryParse(request.To, out var mailbox) || mailbox is not { Address.Length: > 0 })
        {
            throw new EmailCannotBeRendered("The address the email is for is not an email address.");
        }

        // Rendering here is the gate: an unknown or missing value, a bad template or a missing image stops the email before
        // it is queued, so what is queued can be sent.
        renderer.Render(request.Kind, request.Language, request.Values);

        var id = EmailId.New();
        var queued = new EmailQueued(
            id,
            request.Kind,
            request.Language ?? EmailLanguage.NotSet,
            mailbox.Address,
            request.ToName,
            EmailValues.From(request.Values),
            time.GetUtcNow());

        var store = await client.GetEventStore(EventStores.Name, options.Value.Namespace).WaitAsync(cancellationToken);
        var result = request.Subject is null
            ? await store.EventLog.Append(request.About, queued)
            : await store.EventLog.Append(request.About, queued, subject: request.Subject);
        if (!result.IsSuccess)
        {
            throw new EmailNotQueued(typeof(EmailQueued), request.About, result);
        }

        return id;
    }

    /// <inheritdoc/>
    public async Task Bounced(EventSourceId about, EmailId emailId, EmailKind kind, FailureReason reason, CancellationToken cancellationToken = default)
    {
        var store = await client.GetEventStore(EventStores.Name, options.Value.Namespace).WaitAsync(cancellationToken);
        var result = await store.EventLog.Append(about, new EmailBounced(emailId, kind, reason));
        if (!result.IsSuccess)
        {
            throw new EmailNotQueued(typeof(EmailBounced), about, result);
        }
    }
}
