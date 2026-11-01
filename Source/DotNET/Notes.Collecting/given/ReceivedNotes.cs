// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Receiving;

namespace Cratis.Showrunner.Notes.Collecting.given;

/// <summary>
/// Notes, contexts and events the specifications use.
/// </summary>
public static class ReceivedNotes
{
    /// <summary>
    /// The application store the notes come from.
    /// </summary>
    public static readonly AppStoreName AppStore = "Ledger";

    /// <summary>
    /// Where in the application the giver was.
    /// </summary>
    public static readonly SliceLocation Location = new("Billing", "Invoices", "SendInvoice");

    /// <summary>
    /// The giver.
    /// </summary>
    public static readonly NoteGiver Giver = new("8c2f6a1e-giver", "Ada Lovelace");

    /// <summary>
    /// What the giver was looking at.
    /// </summary>
    public static readonly NoteContext Context = new(
        Location,
        [Location],
        new("invoices", "Invoices", "/billing/invoices"),
        "/billing/invoices",
        NoteDialog.NoDialog,
        new Dictionary<string, string> { ["event-model"] = "billing" },
        new CorrelationId(Guid.Empty));

    /// <summary>
    /// The component the giver picked.
    /// </summary>
    public static readonly PickedComponent Component = new("SendButton", "send-invoice", ComponentPart.NotSet, ["InvoiceForm"], Location);

    /// <summary>
    /// The application.
    /// </summary>
    public static readonly PublishedApplication Application = new("Ledger", "1.2.3", "pub-7f3a");

    /// <summary>
    /// The private fact that a note about the context was received.
    /// </summary>
    public static NoteOnContextReceived OnContext { get; } = new(AppStore, Application, Giver, Context, NoteKind.Problem, "The invoice is not sent.");
}
#endif
