// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using Cratis.Showrunner.StageManager.Pulumi.given;
using OperatorPerformanceSpec = Cratis.Showrunner.StageManager.Performances.PerformanceSpec;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances.for_Performance;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_pages_from_a_directory : Specification
{
    string _directory;
    DeclaredResources _declared;
    OperatorPerformanceSpec _spec;

    void Establish()
    {
        _directory = Path.Combine(Path.GetTempPath(), $"authproxy-pages-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_directory);
        File.WriteAllText(Path.Combine(_directory, "403.html"), "<h1>No</h1>");
        File.WriteAllText(Path.Combine(_directory, "styles.css"), "body {}");
    }

    async Task Because()
    {
        _declared = await DeclaredResources.Run(() => _ = new Performance(
            "console",
            new PerformanceArgs
            {
                Namespace = PulumiSpecs.Namespace,
                AuthProxy = new AuthProxyArgs
                {
                    Providers = [new AuthProxyProviderArgs { Kind = AuthProxyProviderKind.GitHub, Identity = "GitHub", CredentialsSecretName = "console-github" }],
                    Pages = new AuthProxyPagesArgs { Directory = _directory },
                    Upstream = new AuthProxyUpstreamArgs { Url = "http://grafana/" },
                },
            }));
        _spec = _declared.SpecOf<OperatorPerformanceSpec>("Performance");
    }

    void Destroy() => Directory.Delete(_directory, true);

    [Fact] void should_write_one_config_map() => _declared.ConfigMaps.Single().KubernetesName.ShouldEqual("console-authproxy-pages");
    [Fact] void should_refer_to_it() => _spec.AuthProxy.Pages!.ConfigMapRef.Name.ShouldEqual("console-authproxy-pages");
}
#endif
