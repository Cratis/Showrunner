// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// The effective configuration of one instance at one revision - what it checks, what it may fix and how, and the
/// secrets its checks and remedies authenticate with.
/// </summary>
/// <param name="InstanceName">The instance it was issued for.</param>
/// <param name="Revision">The monotonic revision.</param>
/// <param name="IssuedAt">When Direct issued it.</param>
/// <param name="ExpiresAt">After this, nothing in it may be acted on offline.</param>
/// <param name="Digest">The <see cref="ConfigurationDigest"/> of its content.</param>
/// <param name="Checks">The checks.</param>
/// <param name="Remedies">The remedies.</param>
/// <param name="Secrets">Credential reference to secret. Revealed only to the authenticated instance.</param>
/// <param name="Offline">The offline policy.</param>
public record WatchdogConfigurationSnapshot(
    string InstanceName,
    long Revision,
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiresAt,
    string Digest,
    IReadOnlyList<EndpointCheckDefinition> Checks,
    IReadOnlyList<RemedyDefinition> Remedies,
    IReadOnlyDictionary<string, string> Secrets,
    OfflinePolicy Offline);
