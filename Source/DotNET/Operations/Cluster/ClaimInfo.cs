// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// A persistent volume claim.
/// </summary>
/// <param name="Namespace">The namespace.</param>
/// <param name="Name">The name.</param>
/// <param name="Phase">The phase, for example <c>Bound</c>.</param>
public sealed record ClaimInfo(string Namespace, string Name, string Phase);
