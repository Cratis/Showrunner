// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Collecting.Receiving;
using Cratis.Showrunner.Notes.Collecting.Responding.AcknowledgeNote;
using Cratis.Showrunner.Notes.Collecting.Responding.AddressNote;
using Cratis.Showrunner.Notes.Collecting.Responding.DeclineNote;
using Cratis.Showrunner.Notes.Contracts.Applications;
using Cratis.Showrunner.Notes.Contracts.Components;
using Cratis.Showrunner.Notes.Contracts.Context;
using Cratis.Showrunner.Notes.Contracts.Giving;
using Cratis.Showrunner.Notes.Contracts.People;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnComponent;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnContext;
using Cratis.Showrunner.Notes.Giving.SuggestComponentPropertyChange;
using Cratis.Showrunner.Notes.Identity;
using Cratis.Showrunner.Notes.Integration.given;
using Cratis.Showrunner.Notes.Receiving;
using Cratis.Showrunner.Notes.Withdrawing.WithdrawNote;
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.Integration.given;

namespace Cratis.Showrunner.Notes.Integration.for_Notes.when_an_application_and_a_collector_talk;

/// <summary>
/// A user gives a note in a tenant of the application and suggests a property change in the same picking session; the
/// collecting side records them in the namespace the tenant names, acknowledges the note and addresses it, declines a
/// second note, and honors the withdrawal of a third.
/// </summary>
[Collection(ChronicleCollection.Name)]
public class about_a_note : Specification
{
    static readonly EventStoreNamespaceName _tenant = "fjord-logistics";
    static readonly GiverUserId _userId = "8c2f6a1e-giver";
    static readonly NoteGiver _giver = new(_userId, "Ada Lovelace");
    static readonly Subject _complianceSubject = NoteGivers.For(_userId);
    static readonly PublishedApplication _published = new("Ledger", "1.2.3", "pub-7f3a");
    static readonly SliceLocation _location = new("Invoicing", "Invoices", "SendInvoice");
    static readonly PickedComponent _component = new("SendButton", "send-invoice", ComponentPart.NotSet, ["InvoiceForm"], _location);

    static readonly NoteContext _context = new(
        _location,
        [_location],
        new("invoices", "Invoices", "/invoicing/invoices"),
        "/invoicing/invoices",
        NoteDialog.NoDialog,
        new Dictionary<string, string> { ["event-model"] = "invoicing" },
        new CorrelationId(Guid.Empty));

    Collector _collector;
    Application _application;
    NoteId _noteId;
    NoteId _declinedNoteId;
    NoteId _withdrawnNoteId;
    NoteId _unknownTenantNoteId;

    AppendedEvent _received;
    AppendedEvent _suggestion;
    AppendedEvent _acknowledgedByCollector;
    AppendedEvent _addressedByCollector;
    AppendedEvent _acknowledged;
    AppendedEvent _addressed;
    AppendedEvent _declined;
    AppendedEvent _withdrawal;
    IReadOnlyList<AppendedEvent> _recordedByCollector;
    IReadOnlyList<AppendedEvent> _unknownTenantEvents;
    IReadOnlyList<AppendedEvent> _defaultNamespaceEvents;

    async Task Because()
    {
        var infrastructure = ChronicleInfrastructure.Current;
        _noteId = NoteId.New();
        _declinedNoteId = NoteId.New();
        _withdrawnNoteId = NoteId.New();
        _unknownTenantNoteId = NoteId.New();

        // The application first, so its store name is known to the collecting side; the collecting store has the
        // tenant's namespace, which is where Chronicle forwards what the application publishes in the same namespace.
        var collectorName = new EventStoreName($"Studio-{Guid.NewGuid():N}");
        _application = await Application.Start(infrastructure, collectorName);
        _collector = await Collector.Start(infrastructure, collectorName, _application.Name);
        await _collector.In(_tenant);

        // The user's facts, recorded in the tenant's namespace as Notes' commands record them.
        var tenant = await _application.In(_tenant);
        await tenant.EventLog.Append(_noteId, new GiverSuggestedComponentPropertyChange(_published, _giver, _context, _component, "label", "Send", "Send invoice"), subject: _complianceSubject);
        await tenant.EventLog.Append(_noteId, new GiverGaveNoteOnComponent(_published, _giver, _context, _component, NoteKind.Suggestion, "Make the button say what it does."), subject: _complianceSubject);
        await tenant.EventLog.Append(_declinedNoteId, new GiverGaveNoteOnContext(_published, _giver, _context, NoteKind.Problem, "Everything should be blue."), subject: _complianceSubject);
        await tenant.EventLog.Append(_withdrawnNoteId, new GiverGaveNoteOnContext(_published, _giver, _context, NoteKind.Question, "Where is the export?"), subject: _complianceSubject);
        await tenant.EventLog.Append(_withdrawnNoteId, new GiverWithdrewNote(), subject: _complianceSubject);

        // A note in a namespace the collecting store does not have is never recorded there.
        var unknown = await _application.In("somewhere-else");
        await unknown.EventLog.Append(_unknownTenantNoteId, new GiverGaveNoteOnContext(_published, _giver, _context, NoteKind.Praise, "Lovely."), subject: _complianceSubject);

        _received = await _collector.WaitFor<NoteOnComponentReceived>(_tenant, _noteId);
        _suggestion = await _collector.WaitFor<ComponentPropertyChangeReceived>(_tenant, _noteId);
        await _collector.WaitFor<NoteOnContextReceived>(_tenant, _declinedNoteId);
        _withdrawal = await _collector.WaitFor<NoteWithdrawalReceived>(_tenant, _withdrawnNoteId);

        // The builder answers: the facts the commands record, in the namespace the note arrived in.
        var collecting = await _collector.In(_tenant);
        await collecting.EventLog.Append(_noteId, new OperatorAcknowledgedNote());
        _acknowledgedByCollector = await _collector.WaitFor<OperatorAcknowledgedNote>(_tenant, _noteId);
        await collecting.EventLog.Append(_noteId, new OperatorAddressedNote("2.0.0"));
        _addressedByCollector = await _collector.WaitFor<OperatorAddressedNote>(_tenant, _noteId);
        await collecting.EventLog.Append(_declinedNoteId, new OperatorDeclinedNote("Out of scope."));

        _acknowledged = await _application.WaitFor<ReceiverAcknowledgedNote>(_tenant, _noteId);
        _addressed = await _application.WaitFor<ReceiverAddressedNote>(_tenant, _noteId);
        _declined = await _application.WaitFor<ReceiverDeclinedNote>(_tenant, _declinedNoteId);

        _recordedByCollector = await _collector.Recorded(_tenant, _noteId);
        _unknownTenantEvents = await (await _collector.In(EventStoreNamespaceName.Default)).EventLog.GetFromSequenceNumber(EventSequenceNumber.First, _unknownTenantNoteId);
        _defaultNamespaceEvents = await (await _application.In(EventStoreNamespaceName.Default)).EventLog.GetFromSequenceNumber(EventSequenceNumber.First, _noteId);
    }

    void Destroy()
    {
        _application?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _collector?.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    [Fact] void should_record_the_note_in_the_namespace_the_tenant_names() => _received.Context.Namespace.ShouldEqual(_tenant);
    [Fact] void should_say_which_application_store_it_came_from() => ((NoteOnComponentReceived)_received.Content).AppStore.ShouldEqual(new AppStoreName(_application.Name.Value));
    [Fact] void should_carry_the_text() => ((NoteOnComponentReceived)_received.Content).Text.Value.ShouldEqual("Make the button say what it does.");
    [Fact] void should_carry_the_publication_the_note_is_about() => ((NoteOnComponentReceived)_received.Content).Application.PublicationId.Value.ShouldEqual("pub-7f3a");
    [Fact] void should_carry_the_giver() => ((NoteOnComponentReceived)_received.Content).Giver.UserId.ShouldEqual(_userId);
    [Fact] void should_carry_the_giver_as_the_compliance_subject() => _received.Context.Subject.ShouldEqual(_complianceSubject);
    [Fact] void should_record_the_suggestion_under_the_same_note() => ((ComponentPropertyChangeReceived)_suggestion.Content).Property.Value.ShouldEqual("label");
    [Fact] void should_record_each_fact_of_the_note_once() => _recordedByCollector.GroupBy(_ => _.Content.GetType()).ShouldEachConformTo(_ => _.Count() == 1);
    [Fact] void should_not_record_a_note_for_a_namespace_the_collecting_store_does_not_have() => _unknownTenantEvents.ShouldBeEmpty();
    [Fact] void should_record_the_withdrawal() => _withdrawal.Context.Namespace.ShouldEqual(_tenant);
    [Fact] void should_publish_the_acknowledgement_to_the_application() => _acknowledged.Context.Namespace.ShouldEqual(_tenant);
    [Fact] void should_publish_the_resolution_with_its_version() => ((ReceiverAddressedNote)_addressed.Content).Version.Value.ShouldEqual("2.0.0");
    [Fact] void should_publish_the_decline_with_its_reason() => ((ReceiverDeclinedNote)_declined.Content).Reason.Value.ShouldEqual("Out of scope.");
    [Fact] void should_keep_when_the_collecting_side_acknowledged_the_note() => _acknowledged.Context.Occurred.ShouldEqual(_acknowledgedByCollector.Context.Occurred);
    [Fact] void should_keep_when_the_collecting_side_addressed_the_note() => _addressed.Context.Occurred.ShouldEqual(_addressedByCollector.Context.Occurred);
    [Fact] void should_not_record_anything_in_the_default_namespace() => _defaultNamespaceEvents.ShouldBeEmpty();
}
