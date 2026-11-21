// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Link;

namespace Cratis.Showrunner.Operations.Configuration.for_WatchdogSettingsReader;

public class when_the_direct_link_is_configured : Specification
{
    WatchdogSettings _settings;
    WatchdogSettings _plain;
    WatchdogConfigurationException _incomplete;
    WatchdogConfigurationException _badRemedy;

    void Because()
    {
        _settings = WatchdogSettingsReader.FromEnvironment(
            new Dictionary<string, string>
            {
                ["DIRECT_LINK_URL"] = "https://direct.example.com",
                ["DIRECT_LINK_INSTANCE"] = "showrunner-prod",
                ["DIRECT_LINK_ENROLLMENT_TOKEN"] = "enroll-s3cret",
                ["DIRECT_LINK_RESOURCE"] = "7d3f7f1e-9a53-4e0b-9f0e-6c1b1f2a3b4c",
                ["DIRECT_LINK_REMEDIES"] = "restart-web=shop/web, restart-api=shop/api",
                ["DIRECT_LINK_HTTP_REMEDIES"] = "true",
                ["DIRECT_LINK_OUTBOX_LIMIT"] = "100"
            },
            "operations");
        _plain = WatchdogSettingsReader.FromEnvironment(new Dictionary<string, string>(), "operations");
        _incomplete = (WatchdogConfigurationException)Catch.Exception(() => WatchdogSettingsReader.FromEnvironment(new Dictionary<string, string> { ["DIRECT_LINK_URL"] = "https://direct.example.com" }, "operations"));
        _badRemedy = (WatchdogConfigurationException)Catch.Exception(() => WatchdogSettingsReader.FromEnvironment(
            new Dictionary<string, string> { ["DIRECT_LINK_URL"] = "https://direct.example.com", ["DIRECT_LINK_INSTANCE"] = "x", ["DIRECT_LINK_REMEDIES"] = "restart-web" },
            "operations"));
    }

    [Fact] void should_read_the_instance_and_address() => (_settings.Link!.InstanceName, _settings.Link.BaseUrl.Host).ShouldEqual(("showrunner-prod", "direct.example.com"));
    [Fact] void should_read_the_enrollment_token() => _settings.Link!.EnrollmentToken.ShouldEqual("enroll-s3cret");
    [Fact] void should_read_the_resource() => _settings.Link!.Resource.ShouldEqual(Guid.Parse("7d3f7f1e-9a53-4e0b-9f0e-6c1b1f2a3b4c"));
    [Fact] void should_read_every_remedy_binding() => _settings.Link!.Remedies.Count.ShouldEqual(2);
    [Fact] void should_read_what_a_remedy_restarts() => _settings.Link!.Remedies[0].ShouldEqual(new RemedyBinding("restart-web", "shop", "web"));
    [Fact] void should_read_the_http_remedy_opt_in() => _settings.Link!.AllowHttpRemedies.ShouldBeTrue();
    [Fact] void should_read_the_outbox_limit() => _settings.Link!.OutboxLimit.ShouldEqual(100);
    [Fact] void should_keep_http_remedies_off_by_default() => new DirectLinkOptions { BaseUrl = new Uri("https://d"), InstanceName = "x" }.AllowHttpRemedies.ShouldBeFalse();
    [Fact] void should_have_no_link_unless_configured() => _plain.Link.ShouldBeNull();
    [Fact] void should_refuse_an_address_without_an_instance() => _incomplete.Problems.ShouldContain("The Direct link needs both DIRECT_LINK_URL and DIRECT_LINK_INSTANCE");
    [Fact] void should_refuse_a_malformed_remedy_binding() => _badRemedy.Problems.ShouldContain("DIRECT_LINK_REMEDIES entries must look like remedy=namespace/deployment");
    [Fact] void should_not_echo_the_token() => string.Join(' ', _incomplete.Problems.Concat(_badRemedy.Problems)).ShouldNotContain("enroll-s3cret");
}
#endif
