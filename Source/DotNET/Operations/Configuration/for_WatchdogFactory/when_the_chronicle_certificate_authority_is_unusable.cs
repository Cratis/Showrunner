// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Showrunner.Operations.Configuration.for_WatchdogFactory;

public class when_the_chronicle_certificate_authority_is_unusable : Specification
{
    IReadOnlyCollection<Finding> _findings;
    Exception _creating;

    async Task Because()
    {
        var settings = WatchdogSettingsReader.FromEnvironment(
            new Dictionary<string, string>
            {
                ["CHRONICLE_URL"] = "https://chronicle:8080",
                ["CHRONICLE_CLIENT_ID"] = "watchdog",
                ["CHRONICLE_CLIENT_SECRET"] = "s3cret-value",
                ["CHRONICLE_CA_CERTIFICATE"] = "not a certificate",
                ["WATCHDOG_CHECKS"] = "chronicle-observers"
            },
            "operations");
        var factory = new WatchdogFactory(
            settings,
            new FakeClusterReader(),
            new FakeConfigMapStore(),
            new HttpClient(StubHttpHandler.Answering(HttpStatusCode.OK)),
            null,
            new ManualTimeProvider(),
            NullLoggerFactory.Instance,
            _ => throw new InvalidOperationException("the certificate authority is not a PEM: not a certificate"));
        _creating = await Record.ExceptionAsync(() => Task.FromResult(factory.CreateChecks()));
        _findings = await factory.CreateChecks().Single().Collect(CheckContexts.At(new ManualTimeProvider(), null, null), CancellationToken.None);
    }

    [Fact] void should_not_crash_the_watchdog_while_it_is_put_together() => _creating.ShouldBeNull();
    [Fact] void should_be_a_finding_and_not_a_crash() => _findings.Select(finding => finding.Key).ShouldContainOnly("chronicle:api");
    [Fact] void should_never_put_the_client_secret_in_the_finding() => _findings.Single().Text.ShouldNotContain("s3cret-value");
}
#endif
