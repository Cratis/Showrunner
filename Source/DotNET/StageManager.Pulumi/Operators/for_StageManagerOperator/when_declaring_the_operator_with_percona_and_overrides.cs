// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using System.Collections.Immutable;
using Cratis.Showrunner.StageManager.Pulumi.given;

namespace Cratis.Showrunner.StageManager.Pulumi.Operators.for_StageManagerOperator;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_the_operator_with_percona_and_overrides : Specification
{
    const string HelmRelease = "kubernetes:helm.sh/v3:Release";

    DeclaredResources _declared;
    RecordedResource _operator;
    RecordedResource _percona;
    ImmutableDictionary<string, object> _values;

    async Task Because()
    {
        _declared = await DeclaredResources.Run(() => _ = new StageManagerOperator(
            "operator",
            new StageManagerOperatorArgs
            {
                Version = "2.17.1",
                Namespace = "platform",
                ReleaseName = "showrunner",
                OperatorImageRepository = "mirror.example.com/showrunner/operator",
                UnderstudyImageRepository = "mirror.example.com/showrunner/understudy",
                Chart = "./Source/Helm/showrunner-operator",
                WaitForOperator = false,
                Percona = new StageManagerPerconaArgs { Version = "1.19.0" },
                Values = new Dictionary<string, object>
                {
                    ["image"] = new Dictionary<string, object> { ["pullPolicy"] = "Always" },
                    ["metrics"] = new Dictionary<string, object> { ["port"] = 0 }
                }
            }));
        _operator = _declared.OfType(HelmRelease).Single(release => release.Name == "operator-release");
        _percona = _declared.OfType(HelmRelease).Single(release => release.Name == "operator-percona");
        _values = (ImmutableDictionary<string, object>)_operator.Inputs["values"];
    }

    [Fact] void should_install_the_percona_operator_from_perconas_repository() => (_percona.Inputs["chart"], ((ImmutableDictionary<string, object>)_percona.Inputs["repositoryOpts"])["repo"]).ShouldEqual(("psmdb-operator", "https://percona.github.io/percona-helm-charts/"));
    [Fact] void should_pin_the_percona_chart_version() => _percona.Inputs["version"].ShouldEqual("1.19.0");
    [Fact] void should_let_the_percona_operator_watch_every_namespace() => ((ImmutableDictionary<string, object>)_percona.Inputs["values"])["watchAllNamespaces"].ShouldEqual(true);
    [Fact] void should_run_the_percona_operator_in_its_own_namespace() => _percona.Inputs["namespace"].ShouldEqual("percona-operator");
    [Fact] void should_install_a_chart_from_a_path_without_a_version() => (_operator.Inputs["chart"], _operator.Inputs.ContainsKey("version")).ShouldEqual(("./Source/Helm/showrunner-operator", false));
    [Fact] void should_use_the_namespace_and_release_name_it_was_given() => (_operator.Inputs["namespace"], _operator.Inputs["name"]).ShouldEqual(("platform", "showrunner"));
    [Fact] void should_use_the_image_repository_override_with_the_version_tag() => ((ImmutableDictionary<string, object>)_values["image"]).Select(pair => $"{pair.Key}={pair.Value}").ShouldContainOnly("repository=mirror.example.com/showrunner/operator", "tag=2.17.1", "pullPolicy=Always");
    [Fact] void should_merge_the_passthrough_values_over_the_typed_ones() => Convert.ToInt32(((ImmutableDictionary<string, object>)_values["metrics"])["port"]).ShouldEqual(0);
    [Fact] void should_not_wait_for_the_operator_when_told_not_to() => _operator.Inputs["skipAwait"].ShouldEqual(true);
}
#endif
