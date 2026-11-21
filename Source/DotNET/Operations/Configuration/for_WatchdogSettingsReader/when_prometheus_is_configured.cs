// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Configuration.for_WatchdogSettingsReader;

public class when_prometheus_is_configured : Specification
{
    WatchdogSettings _settings;
    WatchdogConfigurationException _error;

    void Because()
    {
        _settings = WatchdogSettingsReader.FromEnvironment(
            new Dictionary<string, string> { ["PROMETHEUS_URL"] = "http://prometheus:9090", ["PLATFORM_NAMESPACE"] = "platform" },
            "operations");
        static void Read() => WatchdogSettingsReader.FromEnvironment(
            new Dictionary<string, string> { ["WATCHDOG_CHECKS"] = "metrics" },
            "operations");
        _error = (WatchdogConfigurationException)Catch.Exception(Read);
    }

    [Fact] void should_enable_the_metrics_check() => _settings.EnabledChecks.ShouldContain("metrics");
    [Fact] void should_read_the_platform_namespace() => _settings.Metrics.PlatformNamespace.ShouldEqual("platform");
    [Fact] void should_refuse_the_check_without_an_address() => _error.Problems.ShouldContain("the 'metrics' check is enabled but PROMETHEUS_URL is not set");
}
#endif
