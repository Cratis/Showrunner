// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// A block storage volume in the cloud account, as the volume sweeper reads it.
/// </summary>
/// <param name="Id">The volume id, which is the CSI volume handle of the PersistentVolume that uses it.</param>
/// <param name="Title">The volume title; volumes the CSI driver provisions are titled <c>pvc-…</c>.</param>
/// <param name="Created">When the volume was created.</param>
/// <param name="SizeGigabytes">The size in gigabytes.</param>
/// <param name="ClusterId">The cluster id label the CSI driver put on the volume, if any.</param>
public sealed record CloudVolume(string Id, string Title, DateTimeOffset Created, int SizeGigabytes, string? ClusterId);
