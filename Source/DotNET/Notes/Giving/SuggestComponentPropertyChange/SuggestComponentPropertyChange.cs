// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Notes.Identity;
using Cratis.Showrunner.Notes.Notebook;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Giving.SuggestComponentPropertyChange;

/// <summary>
/// The user changed a presentation property of a component in the picker. There is one command per property; the
/// suggestions of one picking session share the note id, which is also the note id of the text they may write about the
/// component. Who suggests it is taken from the signed-in identity, never from the request.
/// </summary>
/// <param name="NoteId">The id of the picking session's note: new, or the one the session's earlier suggestions were made on.</param>
/// <param name="Context">What the user is looking at.</param>
/// <param name="Component">The component the user picked.</param>
/// <param name="Property">The property the user changed.</param>
/// <param name="CurrentValue">The value the property has.</param>
/// <param name="SuggestedValue">The value the user suggests.</param>
[Command]
[Authorize]
public record SuggestComponentPropertyChange(
    NoteId NoteId,
    NoteContext Context,
    PickedComponent Component,
    ComponentProperty Property,
    PropertyValue CurrentValue,
    PropertyValue SuggestedValue)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="giver">The signed-in user.</param>
    /// <param name="options">The <see cref="NotesOptions"/> naming the application.</param>
    /// <returns>The <see cref="GiverSuggestedComponentPropertyChange"/> event, with the user as its compliance subject.</returns>
    public EventForEventSourceId Handle(ISignedInGiver giver, IOptions<NotesOptions> options) =>
        NoteGivers.Fact(
            NoteId,
            giver.UserId,
            new GiverSuggestedComponentPropertyChange(PublishedApplications.Of(options.Value), giver.Giver, Context, Component, Property, CurrentValue, SuggestedValue));
}

/// <summary>
/// Validates <see cref="SuggestComponentPropertyChange"/>.
/// </summary>
public class SuggestComponentPropertyChangeValidator : CommandValidator<SuggestComponentPropertyChange>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SuggestComponentPropertyChangeValidator"/> class.
    /// </summary>
    /// <param name="note">The note with the command's id, <see langword="null"/> when it does not exist yet.</param>
    /// <param name="giver">The signed-in user.</param>
    /// <param name="options">The <see cref="NotesOptions"/>.</param>
    public SuggestComponentPropertyChangeValidator(Note? note, ISignedInGiver giver, IOptions<NotesOptions> options)
    {
        RuleFor(c => c.NoteId).MustBeSwitchedOn(options).MustBeSignedIn(giver).MustBeNewOrTheGiversOpenNote(note, giver);
        RuleFor(c => c.Context).MustBeReasonable();
        RuleFor(c => c.Component).MustBeReasonable();
        RuleFor(c => (string)c.Property).NotEmpty().WithMessage("Name the property you suggest changing.");
        RuleFor(c => (string)c.CurrentValue).MaximumLength(NoteLimits.PropertyValueLength).WithMessage($"Keep the current value within {NoteLimits.PropertyValueLength} characters.");
        RuleFor(c => (string)c.SuggestedValue).MaximumLength(NoteLimits.PropertyValueLength).WithMessage($"Keep the suggested value within {NoteLimits.PropertyValueLength} characters.");
    }
}

/// <summary>
/// The user suggested a new value for a presentation property of a component. It is published as
/// <c>ComponentPropertyChangeSuggested</c>.
/// </summary>
/// <param name="Application">The application and publication the suggestion was given in.</param>
/// <param name="Giver">Who gave it.</param>
/// <param name="Context">What the user was looking at.</param>
/// <param name="Component">The component the user picked.</param>
/// <param name="Property">The property the user changed.</param>
/// <param name="CurrentValue">The value the property has.</param>
/// <param name="SuggestedValue">The value the user suggests.</param>
[EventType]
public record GiverSuggestedComponentPropertyChange(
    PublishedApplication Application,
    NoteGiver Giver,
    NoteContext Context,
    PickedComponent Component,
    ComponentProperty Property,
    PropertyValue CurrentValue,
    PropertyValue SuggestedValue);
