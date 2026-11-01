// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Building;
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;
using k8s.Models;

namespace Cratis.Showrunner.StageOperator.for_ApplicationObjectBuilder;

public class when_checking_what_is_exposed : Specification
{
    DesiredObjects _result = null!;
    int[] _forbiddenPorts = [ApplicationNames.StagePort, ApplicationNames.ChroniclePort];

    void Because() => _result = ApplicationObjectBuilder.Build(Applications.Create(), Applications.NewImage, 1, true, new StageOperatorOptions { NotesCollectorPort = 4318 });

    [Fact] void should_expose_only_the_proxy_port_on_the_service() => Assert.Equal(["http"], _result.Service.Spec.Ports.Select(p => p.Name));
    [Fact] void should_target_the_named_proxy_port() => Assert.Equal("http", _result.Service.Spec.Ports.Single().TargetPort.Value);
    [Fact] void should_never_put_stage_on_the_service() => Assert.DoesNotContain(_result.Service.Spec.Ports, p => _forbiddenPorts.Contains(p.Port));
    [Fact] void should_never_declare_the_stage_or_chronicle_port_on_a_container() => Assert.DoesNotContain(_result.Deployment.Spec.Template.Spec.Containers.SelectMany(c => c.Ports ?? []), p => _forbiddenPorts.Contains(p.ContainerPort));
    [Fact] void should_declare_only_the_proxy_port_on_the_pod() => Assert.Equal(8080, Assert.Single(_result.Deployment.Spec.Template.Spec.Containers.SelectMany(c => c.Ports ?? [])).ContainerPort);
    [Fact] void should_keep_stage_on_the_loopback_interface() => Assert.Contains(_result.Deployment.Spec.Template.Spec.Containers[0].Env, e => e.Name == "ASPNETCORE_URLS" && e.Value.StartsWith("http://127.0.0.1:9090", StringComparison.Ordinal));
    [Fact] void should_route_nothing_to_stage_or_chronicle_from_the_ingress() => Assert.All(_result.Ingress!.Spec.Rules.SelectMany(r => r.Http.Paths), p => Assert.DoesNotContain(p.Backend.Service.Port.Number ?? 0, _forbiddenPorts));
    [Fact] void should_route_the_catch_all_to_the_proxy_service() => Assert.Equal("orders", _result.Ingress!.Spec.Rules.Single().Http.Paths.Single(p => p.Path == "/").Backend.Service.Name);
    [Fact] void should_route_every_denied_path_to_the_service_without_endpoints() => Assert.All(ApplicationNames.DeniedPaths, path => Assert.Equal("orders-denied", _result.Ingress!.Spec.Rules.Single().Http.Paths.Single(p => p.Path == path).Backend.Service.Name));
    [Fact] void should_deny_the_documented_paths() => Assert.Equal(["/scalar", "/openapi", "/workbench", "/read-models", "/stage/load", "/stage/semantic"], ApplicationNames.DeniedPaths);
    [Fact] void should_give_the_denied_service_no_selector() => Assert.Null(_result.DeniedService.Spec.Selector);
    [Fact] void should_use_prefix_matching_for_every_path() => Assert.All(_result.Ingress!.Spec.Rules.Single().Http.Paths, p => Assert.Equal("Prefix", p.PathType));
    [Fact] void should_publish_under_the_base_domain() => Assert.Equal("orders.cratis.app", _result.Ingress!.Spec.Rules.Single().Host);
    [Fact] void should_allow_only_the_proxy_port_into_the_pod() => Assert.Equal([8080], _result.NetworkPolicies[0].Spec.Ingress.Single().Ports.Select(p => int.Parse(p.Port.Value)));
    [Fact] void should_restrict_the_policy_to_the_application_pods() => Assert.Equal("app", _result.NetworkPolicies[0].Spec.PodSelector.MatchLabels[ApplicationNames.ComponentLabel]);
    [Fact] void should_only_let_the_application_reach_the_database() => Assert.Equal("app", _result.NetworkPolicies[1].Spec.Ingress.Single().FromProperty.Single().PodSelector.MatchLabels[ApplicationNames.ComponentLabel]);
    [Fact] void should_not_allow_the_notes_collector_port_while_notes_is_off() => Assert.DoesNotContain(_result.NetworkPolicies[0].Spec.Ingress.Single().Ports, p => int.Parse(p.Port.Value) == 4318);
}
#endif
