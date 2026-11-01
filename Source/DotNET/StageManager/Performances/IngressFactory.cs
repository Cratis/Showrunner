// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Creates the Ingress of a Performance. It routes every host to AuthProxy and nothing else, with a certificate from cert-manager.
/// </summary>
public static class IngressFactory
{
    /// <summary>
    /// Creates the Ingress.
    /// </summary>
    /// <param name="performance">The Performance that owns it.</param>
    /// <returns>The Ingress.</returns>
    public static V1Ingress Create(Performance performance)
    {
        var spec = performance.Spec;
        var names = PerformanceNames.For(performance);
        var metadata = PerformanceMetadata.Create(performance, names.Ingress, "ingress");

        if (spec.Ingress.TlsIssuer is { } issuer)
        {
            var annotation = string.Equals(issuer.Kind, "Issuer", StringComparison.Ordinal) ? "cert-manager.io/issuer" : "cert-manager.io/cluster-issuer";
            metadata.Annotations = new Dictionary<string, string> { [annotation] = issuer.Name };
        }

        var backend = new V1IngressBackend { Service = new() { Name = names.AuthProxy, Port = new() { Name = "http" } } };
        return new()
        {
            ApiVersion = "networking.k8s.io/v1",
            Kind = "Ingress",
            Metadata = metadata,
            Spec = new()
            {
                IngressClassName = spec.Ingress.ClassName,
                Tls = [new() { Hosts = [.. spec.Ingress.Hosts], SecretName = names.TlsSecret }],
                Rules =
                [
                    .. spec.Ingress.Hosts.Select(host => new V1IngressRule
                    {
                        Host = host,
                        Http = new() { Paths = [new() { Path = "/", PathType = "Prefix", Backend = backend }] }
                    })
                ]
            }
        };
    }
}
