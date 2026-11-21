// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// Binds the name of a remedy defined in Direct to a rolling restart of one named Deployment. Direct keeps governing the remedy
/// (its policy, budget, cooldown, evidence and verification); the binding only says what it does here, and the watchdog's RBAC
/// is limited to exactly these Deployments.
/// </summary>
/// <param name="Remedy">The remedy's name in Direct.</param>
/// <param name="Namespace">The namespace of the Deployment.</param>
/// <param name="Deployment">The name of the Deployment.</param>
public sealed record RemedyBinding(string Remedy, string Namespace, string Deployment);
