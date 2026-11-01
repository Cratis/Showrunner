// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.Herald.Sending;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Herald.given;

/// <summary>
/// An invitation email that is queued against an invitation, and the delivery that works through it.
/// </summary>
public class a_queued_email : a_templates_folder
{
    protected const string Password = "s3cret-password-value";
    protected static readonly EventSourceId Invitation = "invitation-1";

    protected EventStoreForTesting _eventStore;
    protected ScriptedTransport _transport;
    protected SteppingTimeProvider _time;
    protected EmailDelivery _delivery;
    protected EmailId _id;

    protected static readonly TimeSpan FirstDelay = TimeSpan.FromSeconds(30);

    protected async Task<IReadOnlyList<object>> Facts()
    {
        var appended = await _eventStore.EventLog.GetFromSequenceNumber(EventSequenceNumber.First, Invitation);
        return [.. appended.Select(item => item.Content)];
    }

    void Establish()
    {
        _options.Smtp = new() { Host = "smtp.example.com", UserName = "ledger", Password = Password };
        _options.Retry = new() { MaxAttempts = 3, InitialDelay = FirstDelay, MaxDelay = TimeSpan.FromHours(1) };
        _eventStore = new();
        var client = Substitute.For<IChronicleClient>();
        client.GetEventStore(Arg.Any<EventStoreName>(), Arg.Any<EventStoreNamespaceName?>()).Returns(_eventStore);
        _transport = new();
        _time = new();
        var wrapped = Options.Create(_options);
        var templates = new Rendering.TemplateStore(wrapped);
        var renderer = new Rendering.EmailRenderer(templates, new Rendering.EmailImages(wrapped), wrapped);
        _delivery = new(client, Options.Create(new ChronicleClientOptions()), wrapped, renderer, _transport, new FailureReasons(wrapped), _time, Microsoft.Extensions.Logging.Abstractions.NullLogger<EmailDelivery>.Instance);

        _id = EmailId.New();
        _eventStore.EventLog.Append(
            Invitation,
            new EmailQueued(_id, "invitation", EmailLanguage.NotSet, "grace@example.com", "Grace", EmailValues.From(InvitationValues), _time.GetUtcNow())).GetAwaiter().GetResult();
    }

    void Destroy() => _eventStore.Dispose();
}
#endif
