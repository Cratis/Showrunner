// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Cratis.Showrunner.Operations.Link;
using k8s;
using k8s.Models;

namespace Cratis.Showrunner.Operations.Kubernetes;

/// <summary>
/// Restarts a Deployment by stamping its pod template, as <c>kubectl rollout restart</c> does. The watchdog's service account
/// needs <c>get</c> and <c>patch</c> on exactly the Deployments it is bound to, in their namespaces, and only when remedies are enabled.
/// </summary>
/// <param name="client">The Kubernetes client.</param>
/// <param name="timeProvider">The clock.</param>
public sealed class KubernetesDeployments(IKubernetes client, TimeProvider timeProvider) : IDeployments
{
    /// <inheritdoc/>
    public async Task RolloutRestart(string @namespace, string name, CancellationToken cancellationToken)
    {
        var patch = new V1Deployment
        {
            Spec = new V1DeploymentSpec
            {
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta
                    {
                        Annotations = new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["kubectl.kubernetes.io/restartedAt"] = timeProvider.GetUtcNow().ToString("O", CultureInfo.InvariantCulture)
                        }
                    }
                }
            }
        };
        await client.AppsV1.PatchNamespacedDeploymentAsync(new V1Patch(patch, V1Patch.PatchType.MergePatch), name, @namespace, cancellationToken: cancellationToken);
    }
}
