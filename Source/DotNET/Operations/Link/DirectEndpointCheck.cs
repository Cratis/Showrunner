// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http.Headers;
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Link.Protocol;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// Runs the endpoint checks Direct's signed configuration defines, exactly as pinned - method, URL, expected status range,
/// timeout and bearer credential - on top of the watchdog's local checks. A failing check is a finding like any other, so the alert
/// channels see it too; its result also goes to the link, which reports it to Direct. With no configuration (none yet, expired, or
/// no link) it checks nothing.
/// </summary>
/// <param name="link">The link that holds the configuration.</param>
/// <param name="http">The HTTP client; it must not follow redirects.</param>
public sealed class DirectEndpointCheck(DirectLink link, HttpClient http) : IWatchdogCheck
{
    /// <summary>
    /// The name of the check.
    /// </summary>
    public const string CheckName = "direct-endpoints";

    /// <inheritdoc/>
    public string Name => CheckName;

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Finding>> Collect(CheckContext context, CancellationToken cancellationToken)
    {
        if (link.ActiveConfiguration is not { } configuration)
        {
            return [];
        }

        var findings = new List<Finding>();
        foreach (var check in configuration.Checks)
        {
            var result = await Run(check, configuration.Secrets, cancellationToken);
            link.RecordEndpoint(result);
            if (!result.Passed)
            {
                findings.Add(new Finding($"{LinkObservations.EndpointPrefix}{check.Name}", $"Direct check '{check.Name}' failed: {result.Reason}", AlertSeverity.Warning));
            }
        }

        return findings;
    }

    async Task<EndpointCheckResult> Run(EndpointCheckDefinition check, IReadOnlyDictionary<string, string> secrets, CancellationToken cancellationToken)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cancellation.CancelAfter(check.Timeout > TimeSpan.Zero ? check.Timeout : TimeSpan.FromSeconds(10));
        try
        {
            using var request = new HttpRequestMessage(check.Method == WatchdogHttpMethod.Head ? HttpMethod.Head : HttpMethod.Get, check.BaseUrl.TrimEnd('/') + check.Path);
            if (!string.IsNullOrEmpty(check.Credential) && secrets.TryGetValue(check.Credential, out var secret))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secret);
            }

            using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellation.Token);
            var status = (int)response.StatusCode;
            var passed = status >= check.ExpectedStatusFrom && status <= check.ExpectedStatusTo;
            return new EndpointCheckResult(check.Name, check.Resource, passed, status, passed ? string.Empty : $"status {status} is outside {check.ExpectedStatusFrom}-{check.ExpectedStatusTo}");
        }
        catch (Exception error) when (error is HttpRequestException or OperationCanceledException && !cancellationToken.IsCancellationRequested)
        {
            return new EndpointCheckResult(check.Name, check.Resource, false, 0, $"no answer: {error.GetType().Name}");
        }
    }
}
