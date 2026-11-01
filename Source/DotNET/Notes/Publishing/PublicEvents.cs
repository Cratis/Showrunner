// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Giving.GiveNoteOnComponent;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnContext;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnScreen;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnSlices;
using Cratis.Showrunner.Notes.Giving.SuggestComponentPropertyChange;
using Cratis.Showrunner.Notes.Withdrawing.WithdrawNote;

namespace Cratis.Showrunner.Notes.Publishing;

/// <summary>
/// Turns the giver's private facts into the public events the receiving side reads. Each method is one row of the mapping
/// table in the contracts documentation.
/// </summary>
public static class PublicEvents
{
    /// <summary>
    /// Maps a note about what was on screen.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="tenantId">The tenant the note belongs to.</param>
    /// <returns>The public event.</returns>
    public static NoteOnContextGiven For(GiverGaveNoteOnContext @event, TenantId tenantId) =>
        new(tenantId, @event.Application, @event.Giver, @event.Context, @event.Kind, @event.Text);

    /// <summary>
    /// Maps a note about slices.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="tenantId">The tenant the note belongs to.</param>
    /// <returns>The public event.</returns>
    public static NoteOnSlicesGiven For(GiverGaveNoteOnSlices @event, TenantId tenantId) =>
        new(tenantId, @event.Application, @event.Giver, @event.Context, @event.Slices, @event.Kind, @event.Text);

    /// <summary>
    /// Maps a note about the screen.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="tenantId">The tenant the note belongs to.</param>
    /// <returns>The public event.</returns>
    public static NoteOnScreenGiven For(GiverGaveNoteOnScreen @event, TenantId tenantId) =>
        new(tenantId, @event.Application, @event.Giver, @event.Context, @event.Kind, @event.Text);

    /// <summary>
    /// Maps a note about a component.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="tenantId">The tenant the note belongs to.</param>
    /// <returns>The public event.</returns>
    public static NoteOnComponentGiven For(GiverGaveNoteOnComponent @event, TenantId tenantId) =>
        new(tenantId, @event.Application, @event.Giver, @event.Context, @event.Component, @event.Kind, @event.Text);

    /// <summary>
    /// Maps a suggested property change.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="tenantId">The tenant the note belongs to.</param>
    /// <returns>The public event.</returns>
    public static ComponentPropertyChangeSuggested For(GiverSuggestedComponentPropertyChange @event, TenantId tenantId) =>
        new(tenantId, @event.Application, @event.Giver, @event.Context, @event.Component, @event.Property, @event.CurrentValue, @event.SuggestedValue);

    /// <summary>
    /// Maps the giver withdrawing the note.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="tenantId">The tenant the note belongs to.</param>
    /// <returns>The public event.</returns>
    public static NoteWithdrawn For(GiverWithdrewNote @event, TenantId tenantId) => new(tenantId);
}
