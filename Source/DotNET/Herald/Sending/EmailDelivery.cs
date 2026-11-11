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
public class EmailDelivery
{
    readonly IChronicleClient _client;
    readonly IOptions<ChronicleClientOptions> _chronicle;
    readonly IOptions<HeraldOptions> _options;
    readonly EmailDispatch _dispatch;
    readonly TimeProvider _time;
    readonly ILogger<EmailDelivery> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailDelivery"/> class.
    /// </summary>
    /// <param name="client">The <see cref="IChronicleClient"/>.</param>
    /// <param name="chronicle">The <see cref="ChronicleClientOptions"/>, which name the event store when nothing else does.</param>
    /// <param name="options">The <see cref="HeraldOptions"/>.</param>
    /// <param name="renderer">The <see cref="EmailRenderer"/>.</param>
    /// <param name="transport">The <see cref="IEmailTransport"/>.</param>
    /// <param name="reasons">The <see cref="FailureReasons"/>.</param>
    /// <param name="time">The <see cref="TimeProvider"/>.</param>
    /// <param name="logger">The logger.</param>
    public EmailDelivery(
        IChronicleClient client,
        IOptions<ChronicleClientOptions> chronicle,
        IOptions<HeraldOptions> options,
        EmailRenderer renderer,
        IEmailTransport transport,
        FailureReasons reasons,
        TimeProvider time,
        ILogger<EmailDelivery> logger)
        : this(client, chronicle, options, new EmailDispatch(renderer, transport, reasons, options), time, logger)
    {
    }

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

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailDelivery"/> class with the dispatch collaborators grouped.
    /// </summary>
    /// <param name="client">The Chronicle client.</param>
    /// <param name="chronicle">The default event store options.</param>
    /// <param name="options">The Herald options.</param>
    /// <param name="dispatch">The rendering, sending and failure-description service.</param>
    /// <param name="time">The time provider.</param>
    /// <param name="logger">The logger.</param>
    internal EmailDelivery(
        IChronicleClient client,
        IOptions<ChronicleClientOptions> chronicle,
        IOptions<HeraldOptions> options,
        EmailDispatch dispatch,
        TimeProvider time,
        ILogger<EmailDelivery> logger)
    {
        _client = client;
        _chronicle = chronicle;
        _options = options;
        _dispatch = dispatch;
        _time = time;
        _logger = logger;
    }

    HeraldEventStore EventStores { get => field ??= HeraldEventStore.ForClientOptions(_chronicle, _options); init; }

    /// <summary>
    /// Delivers every queued email that is due.
    /// </summary>
    /// <param name="cancellationToken">Cancels the delivery.</param>
    /// <returns>How many emails were looked at.</returns>
    public async Task<int> DeliverDue(CancellationToken cancellationToken = default)
    {
        var store = await _client.GetEventStore(EventStores.Name, _options.Value.Namespace).WaitAsync(cancellationToken);
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
        var history = await EmailHistory.Read(store, about, id);
        if (history.Settled || history.Queued is null)
        {
            return;
        }

        if (history.NextAttemptAt > _time.GetUtcNow())
        {
            return;
        }

        var target = new DeliveryTarget(store, about, history.Queued);
        var attempt = history.Failures + 1;
        if (await TryRender(target, history.Failures, cancellationToken) is not { } rendered || !await TrySend(target, rendered, attempt, cancellationToken))
        {
            return;
        }

        await RecordSent(target, rendered, attempt);
    }

    async Task<RenderedEmail?> TryRender(DeliveryTarget target, int failures, CancellationToken cancellationToken)
    {
        try
        {
            return _dispatch.Render(target.Queued);
        }
        catch (EmailCannotBeRendered exception)
        {
            await Fail(target, exception, failures, cancellationToken);
            return null;
        }
    }

    async Task<bool> TrySend(DeliveryTarget target, RenderedEmail rendered, int attempt, CancellationToken cancellationToken)
    {
        try
        {
            await _dispatch.Send(target.Queued, rendered, cancellationToken);
            return true;
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            await Retry(target, exception, attempt, cancellationToken);
            return false;
        }
    }

    async Task RecordSent(DeliveryTarget target, RenderedEmail rendered, int attempt)
    {
        var queued = target.Queued;
        var result = await target.Store.EventLog.AppendOnce(
            target.About,
            new EmailSent(queued.EmailId, queued.Kind, rendered.Language, attempt),
            new IdempotencyKey($"herald-sent-{queued.EmailId.Value:N}"));
        if (!result.IsSuccess && !result.WasAlreadyAppended)
        {
            throw new EmailNotQueued(typeof(EmailSent), target.About, result);
        }

        _logger.Sent(queued.EmailId, queued.Kind, attempt);
    }

    async Task Retry(DeliveryTarget target, Exception exception, int attempt, CancellationToken cancellationToken)
    {
        var (store, about, queued) = target;
        var permanent = exception is EmailTransportException { Permanent: true };
        if (permanent || attempt >= _options.Value.Retry.MaxAttempts)
        {
            await Fail(target, exception, attempt, cancellationToken);
            return;
        }

        var reason = _dispatch.ReasonFor(exception, queued);
        var next = _time.GetUtcNow() + _options.Value.Retry.DelayAfter(attempt);
        var result = await store.EventLog.Append(about, new EmailAttemptFailed(queued.EmailId, attempt, reason, next));
        if (!result.IsSuccess)
        {
            throw new EmailNotQueued(typeof(EmailAttemptFailed), about, result);
        }

        _logger.WillRetry(queued.EmailId, queued.Kind, attempt, next, reason.Value);
    }

    async Task Fail(DeliveryTarget target, Exception exception, int attempts, CancellationToken cancellationToken)
    {
        var (store, about, queued) = target;
        cancellationToken.ThrowIfCancellationRequested();
        var reason = _dispatch.ReasonFor(exception, queued);
        var result = await store.EventLog.AppendOnce(
            about,
            new EmailFailed(queued.EmailId, queued.Kind, queued.Language, reason, attempts),
            new IdempotencyKey($"herald-failed-{queued.EmailId.Value:N}"));
        if (!result.IsSuccess && !result.WasAlreadyAppended)
        {
            throw new EmailNotQueued(typeof(EmailFailed), about, result);
        }

        _logger.Failed(queued.EmailId, queued.Kind, attempts, reason.Value);
    }

    readonly record struct DeliveryTarget(IEventStore Store, EventSourceId About, EmailQueued Queued);
}
