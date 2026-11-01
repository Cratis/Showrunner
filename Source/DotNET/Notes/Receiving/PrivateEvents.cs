// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Contracts.Responses;

namespace Cratis.Showrunner.Notes.Receiving;

/// <summary>
/// Turns the public events the receiving side sends into the private facts the application records. Each method is one
/// row of the mapping table in the contracts documentation.
/// </summary>
public static class PrivateEvents
{
    /// <summary>
    /// Gets the public events the application receives, and subscribes to.
    /// </summary>
    public static IReadOnlyList<Type> FromReceiver { get; } = [typeof(NoteAcknowledged), typeof(NoteAddressed), typeof(NoteDeclined)];

    /// <summary>
    /// Translates a public event the receiving side sent.
    /// </summary>
    /// <param name="receiverEvent">The public event.</param>
    /// <returns>The tenant it names and the private fact, or <see langword="null"/> when it is not an event the receiving side sends.</returns>
    public static (TenantId TenantId, object Fact)? For(object receiverEvent) => receiverEvent switch
    {
        NoteAcknowledged acknowledged => (acknowledged.TenantId, new ReceiverAcknowledgedNote()),
        NoteAddressed addressed => (addressed.TenantId, new ReceiverAddressedNote(addressed.AddressedInVersion)),
        NoteDeclined declined => (declined.TenantId, new ReceiverDeclinedNote(declined.Reason)),
        _ => null
    };
}
