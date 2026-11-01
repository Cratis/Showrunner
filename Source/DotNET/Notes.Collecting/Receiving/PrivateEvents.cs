// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Collecting.Receiving;

/// <summary>
/// Turns the public events an application sends into the private facts the collecting side records. Each method is one row
/// of the mapping table in the contracts documentation.
/// </summary>
public static class PrivateEvents
{
    /// <summary>
    /// Gets the public events the collecting side receives, and subscribes to.
    /// </summary>
    public static IReadOnlyList<Type> FromApplications { get; } =
    [
        typeof(NoteOnContextGiven),
        typeof(NoteOnSlicesGiven),
        typeof(NoteOnScreenGiven),
        typeof(NoteOnComponentGiven),
        typeof(ComponentPropertyChangeSuggested),
        typeof(NoteWithdrawn)
    ];

    /// <summary>
    /// Translates a public event an application sent.
    /// </summary>
    /// <param name="applicationEvent">The public event.</param>
    /// <param name="appStore">The event store of the application it came from.</param>
    /// <returns>The tenant it names and the private fact, or <see langword="null"/> when it is not an event an application sends.</returns>
    public static (TenantId TenantId, object Fact)? For(object applicationEvent, AppStoreName appStore) => applicationEvent switch
    {
        NoteOnContextGiven given => (given.TenantId, new NoteOnContextReceived(appStore, given.Application, given.Giver, given.Context, given.Kind, given.Text)),
        NoteOnSlicesGiven given => (given.TenantId, new NoteOnSlicesReceived(appStore, given.Application, given.Giver, given.Context, given.Slices, given.Kind, given.Text)),
        NoteOnScreenGiven given => (given.TenantId, new NoteOnScreenReceived(appStore, given.Application, given.Giver, given.Context, given.Kind, given.Text)),
        NoteOnComponentGiven given => (given.TenantId, new NoteOnComponentReceived(appStore, given.Application, given.Giver, given.Context, given.Component, given.Kind, given.Text)),
        ComponentPropertyChangeSuggested suggested => (suggested.TenantId, new ComponentPropertyChangeReceived(appStore, suggested.Application, suggested.Giver, suggested.Context, suggested.Component, suggested.Property, suggested.CurrentValue, suggested.SuggestedValue)),
        NoteWithdrawn withdrawn => (withdrawn.TenantId, new NoteWithdrawalReceived()),
        _ => null
    };
}
