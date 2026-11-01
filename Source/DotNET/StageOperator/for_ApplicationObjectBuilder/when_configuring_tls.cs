// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Building;
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;

namespace Cratis.Showrunner.StageOperator.for_ApplicationObjectBuilder;

public class when_configuring_tls : Specification
{
    DesiredObjects _perHost = null!;
    DesiredObjects _wildcard = null!;
    DesiredObjects _snippet = null!;
    DesiredObjects _stopped = null!;

    void Because()
    {
        var application = Applications.Create();
        _perHost = ApplicationObjectBuilder.Build(application, Applications.NewImage, 1, true, new StageOperatorOptions { TlsIssuerAnnotation = "letsencrypt" });
        _wildcard = ApplicationObjectBuilder.Build(application, Applications.NewImage, 1, true, new StageOperatorOptions { TlsSecretName = "wildcard-tls", TlsIssuerAnnotation = "letsencrypt" });
        _snippet = ApplicationObjectBuilder.Build(application, Applications.NewImage, 1, true, new StageOperatorOptions { DenyPathsWithSnippet = true });
        _stopped = ApplicationObjectBuilder.Build(application, Applications.NewImage, 0, false, new StageOperatorOptions());
    }

    [Fact] void should_request_a_certificate_per_host_from_the_issuer() => Assert.Equal("letsencrypt", _perHost.Ingress!.Metadata.Annotations["cert-manager.io/cluster-issuer"]);
    [Fact] void should_store_the_certificate_in_a_secret_named_after_the_application() => Assert.Equal("orders-tls", _perHost.Ingress!.Spec.Tls.Single().SecretName);
    [Fact] void should_use_the_wildcard_secret_when_configured() => Assert.Equal("wildcard-tls", _wildcard.Ingress!.Spec.Tls.Single().SecretName);
    [Fact] void should_not_request_a_certificate_when_a_wildcard_secret_is_used() => Assert.False(_wildcard.Ingress!.Metadata.Annotations.ContainsKey("cert-manager.io/cluster-issuer"));
    [Fact] void should_not_use_a_snippet_by_default() => Assert.False(_perHost.Ingress!.Metadata.Annotations.ContainsKey("nginx.ingress.kubernetes.io/server-snippet"));
    [Fact] void should_add_the_snippet_when_asked() => Assert.Contains("return 404", _snippet.Ingress!.Metadata.Annotations["nginx.ingress.kubernetes.io/server-snippet"]);
    [Fact] void should_leave_the_ingress_class_to_the_cluster_by_default() => Assert.Null(_perHost.Ingress!.Spec.IngressClassName);
    [Fact] void should_build_no_ingress_when_not_exposed() => Assert.Null(_stopped.Ingress);
    [Fact] void should_scale_to_zero_when_stopped() => Assert.Equal(0, _stopped.Deployment.Spec.Replicas);
}
#endif
