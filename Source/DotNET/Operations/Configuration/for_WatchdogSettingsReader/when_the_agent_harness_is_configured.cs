// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Configuration.for_WatchdogSettingsReader;

public class when_the_agent_harness_is_configured : Specification
{
    WatchdogSettings _enabled;
    WatchdogSettings _observing;

    void Because()
    {
        _enabled = WatchdogSettingsReader.FromEnvironment(new Dictionary<string, string> { ["AGENT_NAMESPACE"] = "direct", ["CLEANUP_ENABLED"] = "true" }, "operations");
        _observing = WatchdogSettingsReader.FromEnvironment(new Dictionary<string, string> { ["AGENT_NAMESPACE"] = "direct" }, "operations");
    }

    [Fact] void should_enable_the_agent_harness_check() => _enabled.EnabledChecks.ShouldContain("agent-harness");
    [Fact] void should_delete_only_when_cleanup_is_explicitly_enabled() => _enabled.AgentHarness!.CleanupEnabled.ShouldBeTrue();
    [Fact] void should_only_observe_by_default() => _observing.AgentHarness!.CleanupEnabled.ShouldBeFalse();
}
#endif
