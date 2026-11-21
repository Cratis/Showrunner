// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// Finds cert-manager Certificates that are not Ready, that expire within fourteen days (renewal is not happening), or whose
/// expiry cannot be read. It needs the cert-manager custom resources: leave it out of a deployment that does not run it.
/// </summary>
/// <param name="reader">The cluster reader.</param>
public sealed class CertificatesCheck(IClusterReader reader) : IWatchdogCheck
{
    /// <summary>
    /// The name of the check in configuration.
    /// </summary>
    public const string CheckName = "certificates";

    static readonly TimeSpan _renewalWindow = TimeSpan.FromDays(14);

    /// <inheritdoc/>
    public string Name => CheckName;

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Finding>> Collect(CheckContext context, CancellationToken cancellationToken)
    {
        IReadOnlyList<CertificateInfo> certificates;
        try
        {
            certificates = await reader.ListCertificates(cancellationToken);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return [new Finding("certificates:api", $"could not read cert-manager Certificates: {Redaction.Summarize(error.Message)}; inspect watchdog RBAC", AlertSeverity.Warning)];
        }

        var findings = new List<Finding>();
        foreach (var certificate in certificates)
        {
            var identity = $"{certificate.Namespace}/{certificate.Name}";
            if (certificate.ReadyStatus != "True")
            {
                findings.Add(new Finding($"certificate:{identity}:ready", $"certificate {identity} is not Ready; inspect cert-manager issuance and renewal events", AlertSeverity.Critical));
                if (certificate.ReadyStatus is not null && certificate.NotAfter is null)
                {
                    // Never issued yet: there is no expiry to read, and the finding above already says why.
                    continue;
                }
            }

            findings.AddRange(Expiry(identity, certificate.NotAfter, context.Now));
        }

        return findings;
    }

    static IEnumerable<Finding> Expiry(string identity, string? notAfter, DateTimeOffset now)
    {
        if (!DateTimeOffset.TryParse(notAfter, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var expires))
        {
            yield return new Finding($"certificate:{identity}:unreadable", $"certificate {identity} expiry unreadable: '{Redaction.Summarize(notAfter)}'; inspect cert-manager status", AlertSeverity.Warning);
            yield break;
        }

        if (expires <= now + _renewalWindow)
        {
            yield return new Finding($"certificate:{identity}:expiry", $"certificate {identity} expires {notAfter}; inspect cert-manager renewal and served certificate rollout", AlertSeverity.Critical);
        }
    }
}
