// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// A configuration snapshot and its signature.
/// </summary>
/// <param name="Snapshot">The snapshot.</param>
/// <param name="Signature">The base64 HMAC-SHA256 of the canonical snapshot under the instance's signing key.</param>
public record SignedWatchdogConfiguration(WatchdogConfigurationSnapshot Snapshot, string Signature);
