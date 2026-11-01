// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using k8s;
using k8s.Models;

namespace Cratis.Showrunner.StageOperator.Building;

/// <summary>Represents every Kubernetes object the operator generates for one application.</summary>
/// <param name="Deployment">The Deployment with the Stage and sign-in proxy containers.</param>
/// <param name="MongoStatefulSet">The MongoDB StatefulSet with its volume claim template.</param>
/// <param name="Service">The Service that exposes the proxy port.</param>
/// <param name="MongoService">The headless Service of MongoDB.</param>
/// <param name="DeniedService">The Service without endpoints that answers the denied paths.</param>
/// <param name="Ingress">The Ingress, or null while the application is not exposed.</param>
/// <param name="NetworkPolicies">The NetworkPolicies of the application and MongoDB pods.</param>
public record DesiredObjects(
    V1Deployment Deployment,
    V1StatefulSet MongoStatefulSet,
    V1Service Service,
    V1Service MongoService,
    V1Service DeniedService,
    V1Ingress? Ingress,
    IReadOnlyList<V1NetworkPolicy> NetworkPolicies)
{
    /// <summary>Gets every generated object.</summary>
    public IEnumerable<IKubernetesObject<V1ObjectMeta>> All
    {
        get
        {
            yield return MongoService;
            yield return MongoStatefulSet;
            yield return DeniedService;
            yield return Service;
            yield return Deployment;
            foreach (var policy in NetworkPolicies)
            {
                yield return policy;
            }

            if (Ingress is not null)
            {
                yield return Ingress;
            }
        }
    }
}
