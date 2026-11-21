// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Checks;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// The GitHub Actions schedule audit against a fake GitHub that the specification fills with repositories, workflows and runs.
/// </summary>
public abstract class an_actions_audit : a_check
{
    protected const string Organization = "/orgs/Cratis/repos?type=all&per_page=100";

    protected FakeGitHubReader _github;
    protected ActionsAuditOptions _options = new();
    protected ActionsScheduleAuditCheck _audit;

    protected override IWatchdogCheck Check => _audit ??= new ActionsScheduleAuditCheck(_github, _options);

    protected static TimeSpan Hours(double hours) => TimeSpan.FromHours(hours);

    protected static string Json(DateTimeOffset moment) => moment.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");

    protected static (long Id, string Path, string State) Active(long id, string path) => (id, path, "active");

    protected void Repositories(params string[] names) => _github.Respond(Organization, names.Select(name => new { full_name = name }).ToArray());

    protected void Workflows(string repository, params (long Id, string Path, string State)[] workflows) =>
        _github.Respond(
            $"/repos/{repository}/actions/workflows?per_page=100",
            new { workflows = workflows.Select(workflow => new { id = workflow.Id, path = workflow.Path, state = workflow.State, updated_at = "2026-06-01T00:00:00Z" }).ToArray() });

    protected void ScheduledRuns(string repository, params (long WorkflowId, TimeSpan Ago)[] runs) =>
        _github.Respond(
            $"/repos/{repository}/actions/runs?event=schedule&per_page=100",
            new { workflow_runs = runs.Select(run => new { workflow_id = run.WorkflowId, created_at = Json(_time.Now - run.Ago) }).ToArray() });

    protected void WorkflowRuns(string repository, long workflow, params TimeSpan[] ago) =>
        _github.Respond(
            $"/repos/{repository}/actions/workflows/{workflow}/runs?event=schedule&per_page=10",
            new { workflow_runs = ago.Select(each => new { created_at = Json(_time.Now - each) }).ToArray() });

    /// <summary>
    /// Makes the repository's one workflow run every hour for the given number of hours, the latest run being <paramref name="lastAgo"/> ago.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="lastAgo">How long ago the latest run was.</param>
    protected void HourlyWorkflow(string repository, TimeSpan lastAgo)
    {
        Workflows(repository, Active(1, ".github/workflows/nightly.yml"));
        ScheduledRuns(repository, (1, lastAgo), (1, lastAgo + Hours(1)), (1, lastAgo + Hours(2)));
    }

    void Establish()
    {
        _github = new FakeGitHubReader();
        Repositories("Cratis/app");
    }
}
#endif
