// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Authoring.Announcements;

/// <summary>
/// The rules the commands that write an announcement share, so each is worded and enforced once.
/// </summary>
static class AnnouncementRules
{
    /// <summary>
    /// Gets the end of an announcement's window: the end that was given, or <see cref="ActiveUntil.Indefinitely"/> when
    /// none was.
    /// </summary>
    /// <param name="until">The end the command carries; the unset default, a date of 0001-01-01, means none.</param>
    /// <returns>The end.</returns>
    internal static ActiveUntil EndOf(ActiveUntil until) =>
        until is null || until.Value == default ? ActiveUntil.Indefinitely : until;

    /// <summary>
    /// Gets the start of an announcement's window: the start that was given, or <see cref="ActiveFrom.Immediately"/> when
    /// none was.
    /// </summary>
    /// <param name="from">The start the command carries; the unset default, a date of 0001-01-01, means none.</param>
    /// <returns>The start.</returns>
    internal static ActiveFrom StartOf(ActiveFrom from) =>
        from is null || from.Value == default ? ActiveFrom.Immediately : from;

    /// <summary>
    /// Gets whether two audiences name the same people: the same kind, and the same tenants, module or plan.
    /// </summary>
    /// <param name="left">One audience.</param>
    /// <param name="right">The other audience.</param>
    /// <returns><see langword="true"/> when they are the same.</returns>
    internal static bool Same(Audience left, Audience right) =>
        left.Kind == right.Kind &&
        left.Module == right.Module &&
        left.Plan == right.Plan &&
        left.TenantIds.ToHashSet().SetEquals(right.TenantIds);

    /// <summary>
    /// Requires an announcement to have a title that fits.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <param name="rule">The rule builder for the command's title.</param>
    /// <returns>The rule builder.</returns>
    internal static IRuleBuilderOptions<T, string> MustBeTitled<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Give the announcement a title.")
            .MaximumLength(AnnouncementLimits.TitleLength).WithMessage($"Keep the title within {AnnouncementLimits.TitleLength} characters.");

    /// <summary>
    /// Requires an announcement's body to fit.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <param name="rule">The rule builder for the command's body.</param>
    /// <returns>The rule builder.</returns>
    internal static IRuleBuilderOptions<T, string> MustFit<T>(this IRuleBuilder<T, string> rule) =>
        rule.MaximumLength(AnnouncementLimits.BodyLength).WithMessage($"Keep the body within {AnnouncementLimits.BodyLength} characters.");

    /// <summary>
    /// Requires the announcement's start to be before its end; an announcement without a start or an end has no window to
    /// get wrong.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <param name="rule">The rule builder for the command.</param>
    /// <param name="from">Gets the start of the window from the command.</param>
    /// <param name="until">Gets the end of the window from the command.</param>
    /// <returns>The rule builder.</returns>
    internal static IRuleBuilderOptions<T, T> MustStartBeforeItEnds<T>(this IRuleBuilder<T, T> rule, Func<T, ActiveFrom> from, Func<T, ActiveUntil> until) =>
        rule.Must(command => StartOf(from(command)).Value < EndOf(until(command)).Value)
            .WithMessage("The announcement has to start before it ends.");

    /// <summary>
    /// Requires an audience to name who it is for: at least one tenant, or the module, or the plan its kind needs.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <param name="rule">The rule builder for the command's audience.</param>
    /// <returns>The rule builder.</returns>
    internal static IRuleBuilderOptions<T, Audience> MustNameWhoItIsFor<T>(this IRuleBuilder<T, Audience> rule) =>
        rule.NotNull().WithMessage("Say who the announcement is for.")
            .Must(audience => audience.Kind != AudienceKind.Tenants || audience.TenantIds.Any()).WithMessage("Name at least one tenant.")
            .Must(audience => audience.Kind != AudienceKind.HoldersOfModule || !string.IsNullOrWhiteSpace(audience.Module.Value)).WithMessage("Name the module whose holders the announcement is for.")
            .Must(audience => audience.Kind != AudienceKind.OnPlan || !string.IsNullOrWhiteSpace(audience.Plan.Value)).WithMessage("Name the plan the announcement is for.");

    /// <summary>
    /// Requires the announcement to be authored, and not withdrawn.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <param name="rule">The rule builder for the command's announcement id.</param>
    /// <param name="announcement">The announcement, or <see langword="null"/> when it is not authored.</param>
    /// <returns>The rule builder.</returns>
    internal static IRuleBuilderOptions<T, Guid> MustBeStanding<T>(this IRuleBuilder<T, Guid> rule, Announcement? announcement) =>
        rule.Must(_ => announcement is not null).WithMessage("This announcement is not authored.")
            .Must(_ => announcement?.Withdrawn != true).WithMessage("This announcement is withdrawn.");

    /// <summary>
    /// Requires the announcement not to be authored yet.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <param name="rule">The rule builder for the command's announcement id.</param>
    /// <param name="announcement">The announcement, or <see langword="null"/> when it is not authored.</param>
    /// <returns>The rule builder.</returns>
    internal static IRuleBuilderOptions<T, Guid> MustNotBeAuthored<T>(this IRuleBuilder<T, Guid> rule, Announcement? announcement) =>
        rule.Must(_ => announcement is null).WithMessage("An announcement with this id is already authored.");
}
