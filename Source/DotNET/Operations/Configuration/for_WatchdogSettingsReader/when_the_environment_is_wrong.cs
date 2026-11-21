// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Configuration.for_WatchdogSettingsReader;

public class when_the_environment_is_wrong : Specification
{
    WatchdogConfigurationException _error;

    void Because()
    {
        static void Read() => WatchdogSettingsReader.FromEnvironment(
            new Dictionary<string, string>
            {
                ["DISCORD_WEBHOOK_URL"] = "not a url but secret-token-1",
                ["SMTP_SERVER"] = "smtp.example.com",
                ["DIRECT_ALERTS_URL"] = "https://direct.example.com",
                ["REALERT_SECONDS"] = "soon",
                ["WATCHDOG_CHECKS"] = "pods,telepathy,pbm-backups",
                ["WATCHDOG_CRONJOBS"] = "broken-entry"
            },
            "ns");
        _error = (WatchdogConfigurationException)Catch.Exception(Read);
    }

    [Fact] void should_refuse_to_run() => (_error is null).ShouldBeFalse();
    [Fact] void should_name_an_invalid_url_variable() => _error.Problems.ShouldContain("DISCORD_WEBHOOK_URL is not an absolute http(s) URL");
    [Fact] void should_name_the_missing_smtp_variables() => _error.Problems.Any(problem => problem.StartsWith("email is configured (SMTP_SERVER) but SMTP_PORT", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_require_the_direct_token_with_the_url() => _error.Problems.ShouldContain("Cratis Direct needs both DIRECT_ALERTS_URL and DIRECT_ALERTS_TOKEN");
    [Fact] void should_name_an_invalid_duration() => _error.Problems.ShouldContain("REALERT_SECONDS is not a duration such as 90s, 30m, 3h or a number of seconds");
    [Fact] void should_name_an_unknown_check() => _error.Problems.Any(problem => problem.Contains("unknown check 'telepathy'", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_say_when_an_enabled_check_has_nothing_to_look_at() => _error.Problems.ShouldContain("the 'pbm-backups' check is enabled but PBM_NAMESPACE is not set");
    [Fact] void should_explain_a_malformed_cronjob_entry() => _error.Problems.Any(problem => problem.Contains("'broken-entry'", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_never_echo_a_value_that_may_be_a_credential() => _error.Message.ShouldNotContain("secret-token-1");
}
#endif
