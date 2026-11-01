// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Herald.Rendering;
using Cratis.Showrunner.Herald.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Herald.Sending;

/// <summary>
/// Delivers the emails that are queued and due, and records the outcome.
/// </summary>
/// <remarks>
/// The event log decides what is waiting, not a read model: a read model can be behind the facts, and an email must not
/// be sent twice because it was. <see cref="QueuedEmail"/> only lists where to look. A delivery that succeeds at the mail
/// server and then fails to record <see cref="EmailSent"/> is delivered again at the next round, so delivery is at least
/// once; run Herald's worker in one instance of the application.
/// </remarks>
/// <param name="client">The <see cref="IChronicleClient"/>.</param>
/// <param name="chronicle">The <see cref="ChronicleClientOptions"/>, which name the event store when nothing else does.</param>
/// <param name="options">The <see cref="HeraldOptions"/>.</param>
/// <param name="renderer">The <see cref="EmailRenderer"/>.</param>
/// <param name="transport">The <see cref="IEmailTransport"/>.</param>
/// <param name="reasons">The <see cref="FailureReasons"/>.</param>
/// <param name="time">The <see cref="TimeProvider"/>.</param>
/// <param name="logger">The logger.</param>
public class EmailDelivery(
    IChronicleClient client,
    IOptions<ChronicleClientOptions> chronicle,
    IOptions<HeraldOptions> options,
    EmailRenderer renderer,
    IEmailTransport transport,
    FailureReasons reasons,
    TimeProvider time,
    ILogger<EmailDelivery> logger)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailDelivery"/> class.
    /// </summary>
    /// <param name="client">The <see cref="IChronicleClient"/>.</param>
    /// <param name="chronicle">The <see cref="ChronicleClientOptions"/>.</param>
    /// <param name="options">The <see cref="HeraldOptions"/>.</param>
    /// <param name="renderer">The <see cref="EmailRenderer"/>.</param>
    /// <param name="transport">The <see cref="IEmailTransport"/>.</param>
    /// <param name="reasons">The <see cref="FailureReasons"/>.</param>
    /// <param name="time">The <see cref="TimeProvider"/>.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="eventStores">The <see cref="HeraldEventStore"/> naming the application's event store.</param>
    public EmailDelivery(
        IChronicleClient client,
        IOptions<ChronicleClientOptions> chronicle,
        IOptions<HeraldOptions> options,
        EmailRenderer renderer,
        IEmailTransport transport,
        FailureReasons reasons,
        TimeProvider time,
        ILogger<EmailDelivery> logger,
        HeraldEventStore eventStores)
        : this(client, chronicle, options, renderer, transport, reasons, time, logger) => EventStores = eventStores;

    HeraldEventStore EventStores { get => field ??= HeraldEventStore.ForClientOptions(chronicle, options); init; }

    /// <summary>
    /// Delivers every queued email that is due.
    /// </summary>
    /// <param name="cancellationToken">Cancels the delivery.</param>
    /// <returns>How many emails were looked at.</returns>
    public async Task<int> DeliverDue(CancellationToken cancellationToken = default)
    {
        var store = await client.GetEventStore(EventStores.Name, options.Value.Namespace).WaitAsync(cancellationToken);
        var queued = (await store.ReadModels.GetInstances<QueuedEmail>()).ToArray();
        foreach (var email in queued.OrderBy(email => email.QueuedAt))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Deliver(store, email.About, email.EmailId, cancellationToken);
        }

        return queued.Length;
    }

    /// <summary>
    /// Delivers one queued email if it is still waiting and its time has come.
    /// </summary>
    /// <param name="store">The event store the email is queued in.</param>
    /// <param name="about">Whatever the email is about, which it is recorded against.</param>
    /// <param name="id">The email.</param>
    /// <param name="cancellationToken">Cancels the delivery.</param>
    /// <returns>An awaitable task.</returns>
    /// <exception cref="EmailNotQueued">The event log did not take the outcome.</exception>
    public async Task Deliver(IEventStore store, EventSourceId about, EmailId id, CancellationToken cancellationToken = default)
    {
        var history = await History(store, about, id);
        if (history.Settled || history.Queued is null || history.NextAttemptAt > time.GetUtcNow())
        {
            return;
        }

        var queued = history.Queued;
        var attempt = history.Failures + 1;
        RenderedEmail rendered;
        try
        {
            rendered = renderer.Render(queued.Kind, queued.Language.Value.Length == 0 ? null : queued.Language, queued.Values.ToDictionary());
        }
        catch (EmailCannotBeRendered exception)
        {
            await Fail(store, about, queued, exception, history.Failures, cancellationToken);
            return;
        }

        try
        {
            await transport.Send(MessageFor(queued, rendered), cancellationToken);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            await Retry(store, about, queued, exception, attempt, cancellationToken);
            return;
        }

        var result = await store.EventLog.AppendOnce(
            about,
            new EmailSent(id, queued.Kind, rendered.Language, attempt),
            new IdempotencyKey($"herald-sent-{id.Value:N}"));
        if (!result.IsSuccess && !result.WasAlreadyAppended)
        {
            throw new EmailNotQueued(typeof(EmailSent), about, result);
        }

        logger.Sent(id, queued.Kind, attempt);
    }

    static async Task<EmailHistory> History(IEventStore store, EventSourceId about, EmailId id)
    {
        var appended = await store.EventLog.GetForEventSourceIdAndEventTypes(
            about,
            [
                store.EventTypes.GetEventTypeFor(typeof(EmailQueued)),
                store.EventTypes.GetEventTypeFor(typeof(EmailAttemptFailed)),
                store.EventTypes.GetEventTypeFor(typeof(EmailSent)),
                store.EventTypes.GetEventTypeFor(typeof(EmailFailed))
            ]);

        EmailQueued? queued = null;
        var failures = 0;
        DateTimeOffset? next = null;
        var settled = false;
        foreach (var content in appended.Select(item => item.Content))
        {
            switch (content)
            {
                case EmailQueued found when found.EmailId == id:
                    queued = found;
                    break;
                case EmailAttemptFailed failed when failed.EmailId == id:
                    failures = Math.Max(failures, failed.Attempt);
                    next = failed.NextAttemptAt;
                    break;
                case EmailSent sent when sent.EmailId == id:
                case EmailFailed failedForGood when failedForGood.EmailId == id:
                    settled = true;
                    break;
            }
        }

        return new(queued, failures, next, settled);
    }

    async Task Retry(IEventStore store, EventSourceId about, EmailQueued queued, Exception exception, int attempt, CancellationToken cancellationToken)
    {
        var permanent = exception is EmailTransportException { Permanent: true };
        if (permanent || attempt >= options.Value.Retry.MaxAttempts)
        {
            await Fail(store, about, queued, exception, attempt, cancellationToken);
            return;
        }

        var reason = reasons.For(exception, queued.Address.Value);
        var next = time.GetUtcNow() + options.Value.Retry.DelayAfter(attempt);
        var result = await store.EventLog.Append(about, new EmailAttemptFailed(queued.EmailId, attempt, reason, next));
        if (!result.IsSuccess)
        {
            throw new EmailNotQueued(typeof(EmailAttemptFailed), about, result);
        }

        logger.WillRetry(queued.EmailId, queued.Kind, attempt, next, reason.Value);
    }

    async Task Fail(IEventStore store, EventSourceId about, EmailQueued queued, Exception exception, int attempts, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var reason = reasons.For(exception, queued.Address.Value);
        var result = await store.EventLog.AppendOnce(
            about,
            new EmailFailed(queued.EmailId, queued.Kind, queued.Language, reason, attempts),
            new IdempotencyKey($"herald-failed-{queued.EmailId.Value:N}"));
        if (!result.IsSuccess && !result.WasAlreadyAppended)
        {
            throw new EmailNotQueued(typeof(EmailFailed), about, result);
        }

        logger.Failed(queued.EmailId, queued.Kind, attempts, reason.Value);
    }

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

    sealed record EmailHistory(EmailQueued? Queued, int Failures, DateTimeOffset? NextAttemptAt, bool Settled);
}
