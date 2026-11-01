// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Giving.GiveNoteOnComponent;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnContext;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnScreen;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnSlices;
using Cratis.Showrunner.Notes.Giving.SuggestComponentPropertyChange;

namespace Cratis.Showrunner.Notes.given;

/// <summary>
/// Notes, contexts and events the specifications use.
/// </summary>
public static class NoteEvents
{
    /// <summary>
    /// Where in the application the giver is.
    /// </summary>
    public static readonly SliceLocation Location = new("Billing", "Invoices", "SendInvoice");

    /// <summary>
    /// Another place in the application.
    /// </summary>
    public static readonly SliceLocation AnotherLocation = new("Billing", "Invoices", "ListInvoices");

    /// <summary>
    /// What the giver is looking at.
    /// </summary>
    public static readonly NoteContext Context = new(
        Location,
        [Location, AnotherLocation],
        new("invoices", "Invoices", "/billing/invoices"),
        "/billing/invoices",
        NoteDialog.NoDialog,
        new Dictionary<string, string> { ["event-model"] = "billing" },
        new CorrelationId(new Guid(0x11111111, 0x2222, 0x3333, 0x44, 0x44, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55)));

    /// <summary>
    /// The component the giver picks.
    /// </summary>
    public static readonly PickedComponent Component = new("SendButton", "send-invoice", ComponentPart.NotSet, ["InvoiceForm", "Page"], Location);

    /// <summary>
    /// The application the notes are given in.
    /// </summary>
    public static readonly PublishedApplication Application = new("Ledger", "1.2.3", "pub-7f3a");

    /// <summary>
    /// A command to give a note about the context.
    /// </summary>
    /// <param name="noteId">The note.</param>
    /// <returns>The command.</returns>
    public static GiveNoteOnContext OnContext(NoteId noteId) => new(noteId, Context, NoteKind.Problem, "The invoice is not sent.");

    /// <summary>
    /// A command to give a note about slices.
    /// </summary>
    /// <param name="noteId">The note.</param>
    /// <returns>The command.</returns>
    public static GiveNoteOnSlices OnSlices(NoteId noteId) => new(noteId, Context, [Location, AnotherLocation], NoteKind.Suggestion, "Make these one screen.");

    /// <summary>
    /// A command to give a note about the screen.
    /// </summary>
    /// <param name="noteId">The note.</param>
    /// <returns>The command.</returns>
    public static GiveNoteOnScreen OnScreen(NoteId noteId) => new(noteId, Context, NoteKind.Question, "Why is this a separate screen?");

    /// <summary>
    /// A command to give a note about a component.
    /// </summary>
    /// <param name="noteId">The note.</param>
    /// <returns>The command.</returns>
    public static GiveNoteOnComponent OnComponent(NoteId noteId) => new(noteId, Context, Component, NoteKind.Praise, "Easy to find.");

    /// <summary>
    /// A command to suggest a property change.
    /// </summary>
    /// <param name="noteId">The note.</param>
    /// <returns>The command.</returns>
    public static SuggestComponentPropertyChange Suggestion(NoteId noteId) => new(noteId, Context, Component, "label", "Send", "Send invoice");

    /// <summary>
    /// The private fact that a note about the context was given by the giver the specifications act as.
    /// </summary>
    /// <param name="giver">Who gave it.</param>
    /// <returns>The event.</returns>
    public static GiverGaveNoteOnContext Given(NoteGiver? giver = null) =>
        new(Application, giver ?? Givers.Giver, Context, NoteKind.Problem, "The invoice is not sent.");

    /// <summary>
    /// The private fact that a property change was suggested.
    /// </summary>
    /// <param name="giver">Who suggested it.</param>
    /// <returns>The event.</returns>
    public static GiverSuggestedComponentPropertyChange Suggested(NoteGiver? giver = null) =>
        new(Application, giver ?? Givers.Giver, Context, Component, "label", "Send", "Send invoice");
}
#endif
