// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Understudies.Discovery;

namespace Cratis.Showrunner.StageManager.Understudies.Catalog;

/// <summary>
/// What the latest backup run found in the cluster, written beside the backups as <c>{production}/plan.json</c> so the operator and the API can report the databases
/// no rule claims without reaching the database server themselves.
/// </summary>
/// <param name="PlannedAt">When the run planned its backups.</param>
/// <param name="Claimed">The databases a rule claims, with what each is.</param>
/// <param name="Unclaimed">The databases no rule claims. They are not backed up.</param>
/// <param name="ServerOwned">The databases the database server owns.</param>
public record PlanReport(DateTimeOffset PlannedAt, IReadOnlyList<PlannedBackup> Claimed, IReadOnlyList<string> Unclaimed, IReadOnlyList<string> ServerOwned)
{
    static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    /// <summary>
    /// Gets the key of the report of a Production.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <returns>The key.</returns>
    public static string KeyOf(string production) => $"{production}/plan.json";

    /// <summary>
    /// Creates the report of a plan.
    /// </summary>
    /// <param name="plan">The plan.</param>
    /// <param name="plannedAt">When it was made.</param>
    /// <returns>The report.</returns>
    public static PlanReport From(BackupPlan plan, DateTimeOffset plannedAt) => new(plannedAt, plan.Backups, plan.Unclaimed, plan.ServerOwned);

    /// <summary>
    /// Writes the report as JSON.
    /// </summary>
    /// <returns>The JSON.</returns>
    public byte[] ToJson() => JsonSerializer.SerializeToUtf8Bytes(this, _options);

    /// <summary>
    /// Reads a report.
    /// </summary>
    /// <param name="json">The JSON stream.</param>
    /// <returns>The report.</returns>
    /// <exception cref="InvalidDataException">Thrown when the content is not a report.</exception>
    public static PlanReport FromJson(Stream json) =>
        JsonSerializer.Deserialize<PlanReport>(json, _options) ?? throw new InvalidDataException("The plan report is empty.");
}
