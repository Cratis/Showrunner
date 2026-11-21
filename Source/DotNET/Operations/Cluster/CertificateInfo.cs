// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// A cert-manager Certificate.
/// </summary>
/// <param name="Namespace">The namespace.</param>
/// <param name="Name">The name.</param>
/// <param name="ReadyStatus">The status of its Ready condition, or <see langword="null"/> when it has none yet.</param>
/// <param name="NotAfter">The expiry as the status states it, or <see langword="null"/> when it was never issued.</param>
public sealed record CertificateInfo(string Namespace, string Name, string? ReadyStatus, string? NotAfter);
