// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using System.Collections.Immutable;
using Cratis.Showrunner.StageManager.Pulumi.given;

namespace Cratis.Showrunner.StageManager.Pulumi.Operators.for_StageManagerOperator;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_the_operator : Specification
{
    const string HelmRelease = "kubernetes:helm.sh/v3:Release";

    DeclaredResources _declared;
    RecordedResource _release;
    ImmutableDictionary<string, object> _values;

    async Task Because()
    {
        _declared = await DeclaredResources.Run(() => _ = new StageManagerOperator(
            "operator",
            new StageManagerOperatorArgs { Version = "2.17.1", ManagedNamespaces = ["apps", "databases"] }));
        _release = _declared.OfType(HelmRelease).Single();
        _values = (ImmutableDictionary<string, object>)_release.Inputs["values"];
    }

    [Fact] void should_declare_a_single_helm_release_and_nothing_else() => _declared.All.Select(resource => resource.Type).Where(type => type != "cratis:showrunner:StageManagerOperator").ShouldContainOnly(HelmRelease);
    [Fact] void should_install_the_published_chart() => _release.Inputs["chart"].ShouldEqual("oci://registry-1.docker.io/cratis/showrunner-operator");
    [Fact] void should_install_the_version_asked_for() => _release.Inputs["version"].ShouldEqual("2.17.1");
    [Fact] void should_install_into_the_operators_namespace_and_create_it() => (_release.Inputs["namespace"], _release.Inputs["createNamespace"]).ShouldEqual(("stage-manager", true));
    [Fact] void should_name_the_release() => _release.Inputs["name"].ShouldEqual("stage-manager");
    [Fact] void should_tag_the_operator_image_with_the_version() => ((ImmutableDictionary<string, object>)_values["image"])["tag"].ShouldEqual("2.17.1");
    [Fact] void should_tag_the_understudy_image_with_the_version() => ((ImmutableDictionary<string, object>)((ImmutableDictionary<string, object>)_values["understudy"])["image"])["tag"].ShouldEqual("2.17.1");
    [Fact] void should_manage_the_namespaces_asked_for() => ((ImmutableArray<object>)_values["managedNamespaces"]).ShouldContainOnly("apps", "databases");
    [Fact] void should_wait_for_the_operator() => _release.Inputs["skipAwait"].ShouldEqual(false);
}
#endif
