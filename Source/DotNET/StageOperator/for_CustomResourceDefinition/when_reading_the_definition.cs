// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.for_CustomResourceDefinition;

public class when_reading_the_definition : Specification
{
    string _crd = string.Empty;
    string _chart = string.Empty;

    void Because()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "Deployment", "Helm", "stage-operator")))
        {
            directory = directory.Parent;
        }

        _chart = Path.Combine(directory!.FullName, "Deployment", "Helm", "stage-operator");
        _crd = File.ReadAllText(Path.Combine(_chart, "crds", "stageapplications.showrunner.cratis.io.yaml"));
    }

    [Fact] void should_be_named_after_the_plural_and_group() => Assert.Contains($"name: {StageApplication.Plural}.{StageApplication.Group}", _crd);
    [Fact] void should_declare_the_kind() => Assert.Contains($"kind: {StageApplication.ResourceKind}", _crd);
    [Fact] void should_declare_the_version() => Assert.Contains($"name: {StageApplication.Version}", _crd);
    [Fact] void should_declare_the_short_name() => Assert.Contains("- stageapp", _crd);
    [Fact] void should_be_namespaced() => Assert.Contains("scope: Namespaced", _crd);
    [Fact] void should_use_the_status_subresource() => Assert.Contains("status: {}", _crd);
    [Fact] void should_require_authentication() => Assert.Contains("                - authentication\n", _crd);
    [Fact] void should_require_a_secret_for_the_proxy() => Assert.Contains("                    - proxyImage\n                    - configSecretRef\n", _crd);
    [Fact] void should_constrain_the_published_name_to_a_dns_label() => Assert.Contains("pattern: '^[a-z0-9]([-a-z0-9]*[a-z0-9])?$'", _crd);
    [Fact] void should_limit_the_published_name_to_63_characters() => Assert.Contains("maxLength: 63", _crd);
    [Fact] void should_require_exactly_one_of_permanent_and_until() => Assert.Contains("(has(self.permanent) && self.permanent) != has(self.until)", _crd);
    [Fact] void should_tie_the_resource_name_to_the_published_name() => Assert.Contains("self.metadata.name == self.spec.publishedName", _crd);
    [Fact] void should_list_every_phase() => Assert.All(typeof(StageApplicationPhases).GetFields().Select(f => (string)f.GetRawConstantValue()!), phase => Assert.Contains(phase, _crd));
    [Fact] void should_cap_the_rollout_history_at_twenty() => Assert.Contains("maxItems: 20", _crd);
    [Fact] void should_keep_the_same_definition_in_the_chart() => Assert.True(File.Exists(Path.Combine(_chart, "Chart.yaml")));
}
#endif
