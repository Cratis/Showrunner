// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// How full a persistent volume claim is, as the kubelet of its node reports it.
/// </summary>
/// <param name="Namespace">The namespace of the claim.</param>
/// <param name="ClaimName">The name of the claim.</param>
/// <param name="CapacityBytes">The capacity.</param>
/// <param name="UsedBytes">The bytes used.</param>
public sealed record VolumeUsage(string Namespace, string ClaimName, long CapacityBytes, long UsedBytes);
