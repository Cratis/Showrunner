// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Alerts;

/// <summary>
/// What a delivery tells the channels: the findings that are new or due again, the ones that escalated, the ones that are still
/// open from an earlier alert, and the ones that resolved since the last run.
/// </summary>
/// <param name="Deployment">The name of the deployment the alert is about, shown in the subject.</param>
/// <param name="Raised">The findings alerted for the first time or after their cooldown.</param>
/// <param name="Escalated">The findings that have been open longer than the escalation threshold.</param>
/// <param name="Ongoing">The findings alerted earlier and still open.</param>
/// <param name="Resolved">The keys of the findings that were open at the last run and are gone.</param>
/// <param name="IsTest">Whether this is a delivery test rather than a real alert.</param>
public sealed record AlertNotification(
    string Deployment,
    IReadOnlyList<Finding> Raised,
    IReadOnlyList<Finding> Escalated,
    IReadOnlyList<Finding> Ongoing,
    IReadOnlyList<string> Resolved,
    bool IsTest = false)
{
    /// <summary>
    /// Gets the findings that need a human now: the raised ones and the escalated ones.
    /// </summary>
    public IEnumerable<Finding> Actionable => Raised.Concat(Escalated);

    /// <summary>
    /// Gets all open findings in the notification.
    /// </summary>
    public IEnumerable<Finding> Open => Raised.Concat(Escalated).Concat(Ongoing);

    /// <summary>
    /// Gets the highest severity among the open findings.
    /// </summary>
    public AlertSeverity Severity => Open.Select(finding => finding.Severity).DefaultIfEmpty(AlertSeverity.Information).Max();

    /// <summary>
    /// Gets the subject: the count of open issues, prefixed when something escalated, or the test label.
    /// </summary>
    public string Subject
    {
        get
        {
            var subject = IsTest
                ? $"[{Deployment}] TEST ALERT — delivery verification"
                : $"[{Deployment}] {Open.Count()} cluster issue(s) need attention";
            return Escalated.Count > 0 ? "ESCALATED: " + subject : subject;
        }
    }

    /// <summary>
    /// Gets the body: escalated issues first, then what is new, then what is still ongoing.
    /// </summary>
    public string Body
    {
        get
        {
            var lines = new List<string>();
            if (Escalated.Count > 0)
            {
                lines.Add("Escalated issues:");
                lines.AddRange(Escalated.OrderBy(finding => finding.Key, StringComparer.Ordinal).Select(finding => $"- {finding.Text}"));
                lines.Add(string.Empty);
            }

            lines.Add(Raised.Count > 0 || Escalated.Count > 0 ? "New since the last alert:" : "Current issues:");
            lines.AddRange(Raised.OrderBy(finding => finding.Key, StringComparer.Ordinal).Select(finding => $"- {finding.Text}"));
            if (Ongoing.Count > 0)
            {
                lines.Add(string.Empty);
                lines.Add("Still ongoing (alerted earlier):");
                lines.AddRange(Ongoing.OrderBy(finding => finding.Key, StringComparer.Ordinal).Select(finding => $"- {finding.Text}"));
            }

            lines.Add(string.Empty);
            lines.Add("From the cluster-health watchdog; one alert per finding per six hours.");
            return string.Join('\n', lines);
        }
    }
}
