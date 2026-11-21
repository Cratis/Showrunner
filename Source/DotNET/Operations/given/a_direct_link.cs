// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using System.Text.Json;
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Link;
using Cratis.Showrunner.Operations.Link.Protocol;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// The Direct link as a CronJob runs it: every pass is a fresh <see cref="DirectLink"/> over the same ConfigMap and Secret, talking
/// the real client to a <see cref="FakeDirectServer"/>, with a manual clock.
/// </summary>
public abstract class a_direct_link : Specification
{
    protected const string Instance = "showrunner-prod";
    protected const string Token = "enrollment-token-value";
    protected const string Restart = "restart-web";
    protected const string Readiness = "readiness";

    protected ManualTimeProvider _time;
    protected FakeDirectServer _direct;
    protected FakeConfigMapStore _configMaps;
    protected FakeSecretStore _secrets;
    protected RecordingDeployments _deployments;
    protected StubHttpHandler _endpoints;
    protected Dictionary<string, HttpStatusCode> _endpointStatus;
    protected DirectLinkOptions _options;
    protected ILinkStore _store;

    protected Guid Resource { get; } = Guid.Parse("7d3f7f1e-9a53-4e0b-9f0e-6c1b1f2a3b4c");

    protected LinkStatus Status => JsonSerializer.Deserialize<LinkStatus>(_configMaps.Values[(_options.StateName, "status")], WatchdogJson.Options)!;

    protected LinkIdentity Identity => JsonSerializer.Deserialize<LinkIdentity>(_secrets.Values[(_options.StateName, "identity")], WatchdogJson.Options)!;

    protected IEnumerable<T> Sent<T>()
        where T : class => _direct.Events.Select(envelope => envelope.PayloadAs<T>()).OfType<T>();

    void Establish()
    {
        _time = new ManualTimeProvider();
        _direct = new FakeDirectServer(Instance, Token, _time);
        _configMaps = new FakeConfigMapStore();
        _secrets = new FakeSecretStore();
        _deployments = new RecordingDeployments();
        _endpointStatus = [];
        _endpoints = new StubHttpHandler(request => new HttpResponseMessage(_endpointStatus.GetValueOrDefault(request.RequestUri!.AbsolutePath, HttpStatusCode.OK)));
        _options = new DirectLinkOptions
        {
            BaseUrl = new Uri("https://direct.example.com"),
            InstanceName = Instance,
            EnrollmentToken = Token,
            Resource = Resource,
            Remedies = [new RemedyBinding(Restart, "shop", "web")],
            RuntimeVersion = "1.2.3"
        };
        _store = new ConfigMapLinkStore(_configMaps, _secrets, _options.StateName);
    }

    protected static Finding Failing(string key) => new(key, $"{key} is failing", AlertSeverity.Warning);

    protected static EndpointCheckDefinition ReadinessCheck(Guid resource) =>
        new(Readiness, resource, WatchdogHttpMethod.Get, "https://shop.internal", "/ready", 200, 299, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), "probe");

    protected static RemedyDefinition RestartRemedy(Guid resource, RemedyPolicyMode online = RemedyPolicyMode.Automatic, RemedyPolicyMode offline = RemedyPolicyMode.Never, int attempts = 2) =>
        new(Restart, resource, WatchdogHttpMethod.Post, "https://shop.internal", "/restart", [], TimeSpan.FromSeconds(10), "admin", Readiness, online, offline, attempts, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(5));

    protected long PublishReadiness(RemedyPolicyMode online = RemedyPolicyMode.Automatic, RemedyPolicyMode offline = RemedyPolicyMode.Never, bool offlineRemedies = false, int attempts = 2) =>
        _direct.Publish([ReadinessCheck(Resource)], [RestartRemedy(Resource, online, offline, attempts)], new OfflinePolicy(true, offlineRemedies), new Dictionary<string, string> { ["probe"] = "probe-secret", ["admin"] = "admin-secret" });

    protected DirectLink CreateLink(DirectLinkOptions? options = null)
    {
        var used = options ?? _options;
        var http = new HttpClient(new FakeDirectHandler(_direct));
        IRemedy[] remedies = [.. used.Remedies.Select(binding => new DeploymentRestartRemedy(binding, _deployments))];
        return new DirectLink(
            used,
            new HttpDirectClient(http, used.BaseUrl, used.InstanceName),
            new ConfigMapLinkStore(_configMaps, _secrets, used.StateName),
            remedies,
            used.AllowHttpRemedies ? name => new HttpRemedy(new HttpClient(_endpoints), name) : null,
            _time,
            NullLogger<DirectLink>.Instance);
    }

    protected async Task<DirectLink> Pass(params Finding[] findings)
    {
        var link = CreateLink();
        await link.Begin(CancellationToken.None);
        var check = new DirectEndpointCheck(link, new HttpClient(_endpoints));
        var collected = await check.Collect(new Pipeline.CheckContext(_time.Now, _time.Now, _time.Now.AddMinutes(1), _time), CancellationToken.None);
        await link.Observe(findings.Concat(collected).ToDictionary(finding => finding.Key), CancellationToken.None);
        await link.End(CancellationToken.None);
        return link;
    }
}
#endif
