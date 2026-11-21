// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Configuration;

/// <summary>
/// How the watchdog reads Chronicle's observer state.
/// </summary>
/// <param name="Url">The Chronicle API address (its TLS API port).</param>
/// <param name="ClientId">The client id of the watchdog's application client.</param>
/// <param name="ClientSecret">The client secret. It is never logged.</param>
/// <param name="CertificateAuthority">The PEM of the certificate authority Chronicle's certificate chains to, when it is not a public one.</param>
public sealed record ChronicleSettings(Uri Url, string ClientId, string ClientSecret, string? CertificateAuthority = null);
