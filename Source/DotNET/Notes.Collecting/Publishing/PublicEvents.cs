// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Collecting.Responding.AcknowledgeNote;
using Cratis.Showrunner.Notes.Collecting.Responding.AddressNote;
using Cratis.Showrunner.Notes.Collecting.Responding.DeclineNote;

namespace Cratis.Showrunner.Notes.Collecting.Publishing;

/// <summary>
/// Turns the builder's private facts into the public events the application reads. Each method is one row of the mapping
/// table in the contracts documentation.
/// </summary>
public static class PublicEvents
{
    /// <summary>
    /// Maps an acknowledged note.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="tenantId">The tenant the note belongs to.</param>
    /// <returns>The public event.</returns>
    public static NoteAcknowledged For(OperatorAcknowledgedNote @event, TenantId tenantId) => new(tenantId);

    /// <summary>
    /// Maps an addressed note.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="tenantId">The tenant the note belongs to.</param>
    /// <returns>The public event.</returns>
    public static NoteAddressed For(OperatorAddressedNote @event, TenantId tenantId) => new(tenantId, @event.Version);

    /// <summary>
    /// Maps a declined note.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="tenantId">The tenant the note belongs to.</param>
    /// <returns>The public event.</returns>
    public static NoteDeclined For(OperatorDeclinedNote @event, TenantId tenantId) => new(tenantId, @event.Reason);
}
