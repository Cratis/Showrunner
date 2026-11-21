// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Probe.for_ProbeTargets;

public class when_reading_targets : Specification
{
    const string Json = "[{\"name\":\"studio\",\"baseUrl\":\"https://app.cratis.studio\",\"note\":\"n\",\"checks\":[{\"id\":\"sse\",\"method\":\"GET\",\"path\":\"/.cratis/queries/sse\",\"accept\":\"text/event-stream\",\"expectStatus\":401},{\"id\":\"sub\",\"method\":\"POST\",\"path\":\"/s\",\"accept\":\"application/json\",\"body\":\"{}\",\"expectStatus\":401,\"timeoutMs\":2000},{\"id\":\"bare\",\"path\":\"/b\",\"expectStatus\":200}]}]";

    IReadOnlyList<ProbeTarget> _targets;
    IReadOnlyList<ProbeTarget> _overridden;
    readonly List<string> _refused = [];

    void Because()
    {
        _targets = ProbeTargets.Parse(Json);
        _overridden = ProbeEvaluator.Override(_targets, ProbeTargets.ParseOverrides("{\"studio\":\"http://127.0.0.1:9\"}"));
        foreach (var bad in new[] { "not json", "[]", "[{\"name\":\"x\",\"baseUrl\":\"ftp://x\",\"checks\":[]}]", "[{\"name\":\"x\",\"baseUrl\":\"https://x\",\"checks\":[]}]", "[{\"name\":\"x\",\"baseUrl\":\"https://x\",\"checks\":[{\"id\":\"a\",\"path\":\"no-slash\",\"expectStatus\":200}]}]", "[{\"name\":\"x\",\"baseUrl\":\"https://x\",\"checks\":[{\"id\":\"a\",\"path\":\"/a\"}]}]" })
        {
            try
            {
                ProbeTargets.Parse(bad);
            }
            catch (InvalidOperationException)
            {
                _refused.Add(bad);
            }
        }
    }

    [Fact] void should_read_the_target() => (_targets.Single().Name, _targets.Single().Checks.Count).ShouldEqual(("studio", 3));
    [Fact] void should_read_the_body_and_the_timeout() => (_targets[0].Checks[1].Body, _targets[0].Checks[1].TimeoutMs).ShouldEqual(("{}", 2000));
    [Fact] void should_default_the_method_and_accept_header() => (_targets[0].Checks[2].Method, _targets[0].Checks[2].Accept).ShouldEqual(("GET", "application/json"));
    [Fact] void should_replace_a_base_address() => _overridden.Single().BaseUrl.ShouldEqual("http://127.0.0.1:9");
    [Fact] void should_refuse_every_invalid_document() => _refused.Count.ShouldEqual(6);
    [Fact] void should_read_no_overrides_from_nothing() => ProbeTargets.ParseOverrides(null).ShouldBeEmpty();
}
#endif
