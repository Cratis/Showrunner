// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Collecting.Notebook;

/// <summary>
/// The rules the collecting commands on a note share, for their validators.
/// </summary>
public static class ReceivedNoteRules
{
    /// <summary>
    /// The note must have been received in the tenant and must not be withdrawn: the giver took it back, so nothing is
    /// acted on it.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <typeparam name="TProperty">The type of the property the rule is for.</typeparam>
    /// <param name="rule">The rule for the command's note id.</param>
    /// <param name="note">The note, or <see langword="null"/> when it was not received.</param>
    /// <returns>The rule for further configuration.</returns>
    public static IRuleBuilderOptions<T, TProperty> MustBeAnOpenNote<T, TProperty>(this IRuleBuilder<T, TProperty> rule, ReceivedNote? note) =>
        rule.Must(_ => note is not null).WithMessage("No note with this id was received.")
            .Must(_ => note is not { IsWithdrawn: true }).WithMessage("The giver withdrew the note, so nothing is acted on it.");
}
