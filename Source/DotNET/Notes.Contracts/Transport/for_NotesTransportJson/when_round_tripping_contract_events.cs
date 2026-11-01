// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text.Json;
using Cratis.Showrunner.Notes.Contracts.Responses;

namespace Cratis.Showrunner.Notes.Contracts.Transport.for_NotesTransportJson;

/// <summary>
/// The collector reads back exactly what the application sent, for every contract the application publishes.
/// </summary>
public class when_round_tripping_contract_events : Specification
{
    static readonly SliceLocation _location = new("Billing", "Invoices", "SendInvoice");
    static readonly PublishedApplication _application = new("Ledger", "1.2.3", "pub-7f3a");
    static readonly NoteGiver _giver = new("8c2f6a1e-giver", "Ada Lovelace");
    static readonly PickedComponent _component = new("SendButton", "send-invoice", ComponentPart.NotSet, ["InvoiceForm"], _location);
    static readonly NoteContext _context = new(
        _location,
        [_location],
        new("invoices", "Invoices", "/billing/invoices"),
        "/billing/invoices",
        NoteDialog.NoDialog,
        new Dictionary<string, string> { ["event-model"] = "billing" },
        new Usher.Contracts.Reporting.CorrelationId(Guid.Parse("11111111-2222-3333-4444-555555555555")));

    object[] _events = [];
    object[] _read = [];
    string _json = string.Empty;

    void Because()
    {
        _events =
        [
            new NoteOnContextGiven("fjord", _application, _giver, _context, NoteKind.Problem, "The invoice is not sent."),
            new NoteOnSlicesGiven("fjord", _application, _giver, _context, [_location], NoteKind.Suggestion, "Make these one screen."),
            new NoteOnScreenGiven("fjord", _application, _giver, _context, NoteKind.Question, "Why a separate screen?"),
            new NoteOnComponentGiven("fjord", _application, _giver, _context, _component, NoteKind.Praise, "Easy to find."),
            new ComponentPropertyChangeSuggested("fjord", _application, _giver, _context, _component, "label", "Send", "Send invoice"),
            new NoteWithdrawn("fjord")
        ];
        _json = JsonSerializer.Serialize(_events.Select(_ => JsonSerializer.SerializeToElement(_, _.GetType(), NotesTransportJson.Options)), NotesTransportJson.Options);
        _read = [.. _events.Select(_ => JsonSerializer.SerializeToElement(_, _.GetType(), NotesTransportJson.Options).Deserialize(_.GetType(), NotesTransportJson.Options)!)];
    }

    [Fact] void should_read_back_every_event() => _read.Length.ShouldEqual(_events.Length);
    [Fact] void should_read_back_equal_events() => _events.Zip(_read).All(pair => JsonSerializer.Serialize(pair.First, pair.First.GetType(), NotesTransportJson.Options) == JsonSerializer.Serialize(pair.Second, pair.Second.GetType(), NotesTransportJson.Options)).ShouldBeTrue();
    [Fact] void should_read_back_the_concepts() => ((NoteOnContextGiven)_read[0]).Giver.ShouldEqual(_giver);
    [Fact] void should_read_back_the_kind() => ((NoteOnContextGiven)_read[0]).Kind.ShouldEqual(NoteKind.Problem);
    [Fact] void should_read_back_the_references() => ((NoteOnContextGiven)_read[0]).Context.ModelReferences["event-model"].ShouldEqual("billing");
    [Fact] void should_write_members_in_camel_case() => _json.ShouldContain("\"tenantId\":\"fjord\"");
    [Fact] void should_write_the_kind_by_name() => _json.ShouldContain("\"kind\":\"Problem\"");
}
#endif
