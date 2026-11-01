// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Identity;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Notebook;

/// <summary>
/// The rules the commands on a note share, for their validators.
/// </summary>
public static class NoteRules
{
    /// <summary>
    /// Notes must be switched on.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <typeparam name="TProperty">The type of the property the rule is for.</typeparam>
    /// <param name="rule">The rule.</param>
    /// <param name="options">The <see cref="NotesOptions"/>.</param>
    /// <returns>The rule for further configuration.</returns>
    public static IRuleBuilderOptions<T, TProperty> MustBeSwitchedOn<T, TProperty>(this IRuleBuilder<T, TProperty> rule, IOptions<NotesOptions> options) =>
        rule.Must(_ => options.Value.Enabled)
            .WithMessage("Notes is switched off in this application.");

    /// <summary>
    /// A user must be signed in; the giver of a note is always the signed-in user.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <typeparam name="TProperty">The type of the property the rule is for.</typeparam>
    /// <param name="rule">The rule.</param>
    /// <param name="giver">The signed-in user.</param>
    /// <returns>The rule for further configuration.</returns>
    public static IRuleBuilderOptions<T, TProperty> MustBeSignedIn<T, TProperty>(this IRuleBuilder<T, TProperty> rule, ISignedInGiver giver) =>
        rule.Must(_ => giver.IsSignedIn)
            .WithMessage("Sign in to give a note.");

    /// <summary>
    /// The note must be new, or one the signed-in user started with suggested property changes and has not yet written the
    /// text of. A note someone else started is indistinguishable from one that is taken, so the id of another person's note
    /// reveals nothing.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <typeparam name="TProperty">The type of the property the rule is for.</typeparam>
    /// <param name="rule">The rule for the command's note id.</param>
    /// <param name="note">The note, or <see langword="null"/> when it does not exist yet.</param>
    /// <param name="giver">The signed-in user.</param>
    /// <returns>The rule for further configuration.</returns>
    public static IRuleBuilderOptions<T, TProperty> MustBeOpenForText<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Note? note, ISignedInGiver giver) =>
        rule.Must(_ => note is null || (note.GivenBy == giver.UserId && !note.TextGiven && !note.IsWithdrawn))
            .WithMessage("A note with this id is already given.");

    /// <summary>
    /// The note must be new, or one the signed-in user gave and has not withdrawn: the user can suggest several property
    /// changes in one picking session.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <typeparam name="TProperty">The type of the property the rule is for.</typeparam>
    /// <param name="rule">The rule for the command's note id.</param>
    /// <param name="note">The note, or <see langword="null"/> when it does not exist yet.</param>
    /// <param name="giver">The signed-in user.</param>
    /// <returns>The rule for further configuration.</returns>
    public static IRuleBuilderOptions<T, TProperty> MustBeNewOrTheGiversOpenNote<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Note? note, ISignedInGiver giver) =>
        rule.Must(_ => note is null || (note.GivenBy == giver.UserId && !note.IsWithdrawn))
            .WithMessage("A note with this id is already given by someone else, or withdrawn.");

    /// <summary>
    /// The note must exist and be given by the signed-in user. A note someone else gave is indistinguishable from one that
    /// does not exist.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <typeparam name="TProperty">The type of the property the rule is for.</typeparam>
    /// <param name="rule">The rule for the command's note id.</param>
    /// <param name="note">The note, or <see langword="null"/> when it does not exist.</param>
    /// <param name="giver">The signed-in user.</param>
    /// <returns>The rule for further configuration.</returns>
    public static IRuleBuilderOptions<T, TProperty> MustBeGivenBy<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Note? note, ISignedInGiver giver) =>
        rule.Must(_ => note is not null && giver.IsSignedIn && note.GivenBy == giver.UserId)
            .WithMessage("There is no note you gave with this id.");

    /// <summary>
    /// The note must not be withdrawn.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <typeparam name="TProperty">The type of the property the rule is for.</typeparam>
    /// <param name="rule">The rule for the command's note id.</param>
    /// <param name="note">The note, or <see langword="null"/> when it does not exist.</param>
    /// <returns>The rule for further configuration.</returns>
    public static IRuleBuilderOptions<T, TProperty> MustNotBeWithdrawn<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Note? note) =>
        rule.Must(_ => note is not { IsWithdrawn: true })
            .WithMessage("The note was withdrawn and can not be changed.");

    /// <summary>
    /// The text of a note must be written, and within <see cref="NoteLimits.TextLength"/> characters.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <param name="rule">The rule for the command's text.</param>
    /// <returns>The rule for further configuration.</returns>
    public static IRuleBuilderOptions<T, string> MustBeNoteText<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Write your note.")
            .MaximumLength(NoteLimits.TextLength).WithMessage($"Keep the note within {NoteLimits.TextLength} characters.");

    /// <summary>
    /// The context must be given and within the limits.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <param name="rule">The rule for the command's context.</param>
    /// <returns>The rule for further configuration.</returns>
    public static IRuleBuilderOptions<T, NoteContext> MustBeReasonable<T>(this IRuleBuilder<T, NoteContext> rule) =>
        rule.NotNull().WithMessage("A note needs to know what you were looking at.")
            .Must(context => context?.ActiveSlices is null || context.ActiveSlices.Count() <= NoteLimits.Slices)
            .WithMessage($"A note can carry at most {NoteLimits.Slices} active slices.")
            .Must(context => context?.ModelReferences is null || context.ModelReferences.Count <= NoteLimits.ModelReferences)
            .WithMessage($"A note can carry at most {NoteLimits.ModelReferences} model references.");

    /// <summary>
    /// The picked component must be given and within the limits.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <param name="rule">The rule for the command's component.</param>
    /// <returns>The rule for further configuration.</returns>
    public static IRuleBuilderOptions<T, PickedComponent> MustBeReasonable<T>(this IRuleBuilder<T, PickedComponent> rule) =>
        rule.NotNull().WithMessage("Pick a component.")
            .Must(component => component?.Ancestors is null || component.Ancestors.Count() <= NoteLimits.Ancestors)
            .WithMessage($"A component can have at most {NoteLimits.Ancestors} ancestors.");
}
