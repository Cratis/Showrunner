// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Configuration.for_WatchdogSettingsReader;

public class when_chronicle_is_configured : Specification
{
    WatchdogSettings _settings;
    WatchdogConfigurationException _error;

    void Because()
    {
        _settings = WatchdogSettingsReader.FromEnvironment(
            new Dictionary<string, string>
            {
                ["CHRONICLE_URL"] = "https://chronicle:8080",
                ["CHRONICLE_CLIENT_ID"] = "watchdog",
                ["CHRONICLE_CLIENT_SECRET"] = "s3cret-value",
                ["CHRONICLE_CA_CERTIFICATE"] = "pem"
            },
            "operations");
        static void Read() => WatchdogSettingsReader.FromEnvironment(new Dictionary<string, string> { ["CHRONICLE_URL"] = "https://chronicle:8080" }, "operations");
        _error = (WatchdogConfigurationException)Catch.Exception(Read);
    }

    [Fact] void should_enable_the_observers_check() => _settings.EnabledChecks.ShouldContain("chronicle-observers");
    [Fact] void should_read_the_client() => (_settings.Chronicle!.ClientId, _settings.Chronicle.CertificateAuthority).ShouldEqual(("watchdog", "pem"));
    [Fact] void should_refuse_an_address_without_credentials() => _error.Problems.ShouldContain("Chronicle needs CHRONICLE_CLIENT_ID and CHRONICLE_CLIENT_SECRET together with CHRONICLE_URL");
    [Fact] void should_not_echo_the_secret() => string.Join(' ', _error.Problems).ShouldNotContain("s3cret-value");
}
#endif
