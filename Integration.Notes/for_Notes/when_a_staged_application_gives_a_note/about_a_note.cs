// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Collecting.Receiving;
using Cratis.Showrunner.Notes.Contracts.Applications;
using Cratis.Showrunner.Notes.Contracts.Context;
using Cratis.Showrunner.Notes.Contracts.Giving;
using Cratis.Showrunner.Notes.Contracts.People;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnContext;
using Cratis.Showrunner.Notes.Identity;
using Cratis.Showrunner.Notes.Integration.given;
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.Integration.given;

namespace Cratis.Showrunner.Notes.Integration.for_Notes.when_a_staged_application_gives_a_note;

/// <summary>
/// An application that runs staged publishes a marker after the note it gives, and the collecting side records it on the
/// note; a published application publishes none, so its note carries no marker.
/// </summary>
[Collection(ChronicleCollection.Name)]
public class about_a_note : Specification
{
    static readonly EventStoreNamespaceName _tenant = "fjord-logistics";
    static readonly GiverUserId _userId = "8c2f6a1e-giver";
    static readonly NoteGiver _giver = new(_userId, "Ada Lovelace");
    static readonly PublishedApplication _published = new("Ledger", "1.2.3", "pub-7f3a");
    static readonly SliceLocation _location = new("Invoicing", "Invoices", "SendInvoice");

    static readonly NoteContext _context = new(
        _location,
        [_location],
        new("invoices", "Invoices", "/invoicing/invoices"),
        "/invoicing/invoices",
        NoteDialog.NoDialog,
        new Dictionary<string, string> { ["event-model"] = "invoicing" },
        new CorrelationId(Guid.Empty));

    Collector _staged;
    Application _stagedApplication;
    Collector _publishedCollector;
    Application _publishedApplication;
    IReadOnlyList<AppendedEvent> _stagedNote;
    IReadOnlyList<AppendedEvent> _publishedNote;

    async Task Because()
    {
        var infrastructure = ChronicleInfrastructure.Current;
        var stagedId = NoteId.New();
        var publishedId = NoteId.New();

        var stagedCollectorName = new EventStoreName($"Studio-{Guid.NewGuid():N}");
        _stagedApplication = await Application.Start(infrastructure, stagedCollectorName, kind: ApplicationKind.Staged);
        _staged = await Collector.Start(infrastructure, stagedCollectorName, _stagedApplication.Name);
        await _staged.In(_tenant);
        await (await _stagedApplication.In(_tenant)).EventLog.Append(stagedId, new GiverGaveNoteOnContext(_published, _giver, _context, NoteKind.Problem, "Staged."), subject: NoteGivers.For(_userId));
        await _staged.WaitFor<NoteOnStagedApplicationReceived>(_tenant, stagedId);
        _stagedNote = await _staged.Recorded(_tenant, stagedId);

        var publishedCollectorName = new EventStoreName($"Studio-{Guid.NewGuid():N}");
        _publishedApplication = await Application.Start(infrastructure, publishedCollectorName);
        _publishedCollector = await Collector.Start(infrastructure, publishedCollectorName, _publishedApplication.Name);
        await _publishedCollector.In(_tenant);
        await (await _publishedApplication.In(_tenant)).EventLog.Append(publishedId, new GiverGaveNoteOnContext(_published, _giver, _context, NoteKind.Problem, "Published."), subject: NoteGivers.For(_userId));
        await _publishedCollector.WaitFor<NoteOnContextReceived>(_tenant, publishedId);

        // The marker follows the note in the outbox, so once a later note has arrived any marker for this one would have.
        var followUp = NoteId.New();
        await (await _publishedApplication.In(_tenant)).EventLog.Append(followUp, new GiverGaveNoteOnContext(_published, _giver, _context, NoteKind.Praise, "Later."), subject: NoteGivers.For(_userId));
        await _publishedCollector.WaitFor<NoteOnContextReceived>(_tenant, followUp);
        _publishedNote = await _publishedCollector.Recorded(_tenant, publishedId);
    }

    void Destroy()
    {
        _stagedApplication?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _staged?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _publishedApplication?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _publishedCollector?.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    [Fact] void should_record_the_marker_on_the_staged_note_once() => _stagedNote.Count(_ => _.Content is NoteOnStagedApplicationReceived).ShouldEqual(1);
    [Fact] void should_record_the_marker_after_the_note() => _stagedNote[^1].Content.ShouldBeOfExactType<NoteOnStagedApplicationReceived>();
    [Fact] void should_record_no_marker_on_a_published_note() => _publishedNote.Count(_ => _.Content is NoteOnStagedApplicationReceived).ShouldEqual(0);
}
