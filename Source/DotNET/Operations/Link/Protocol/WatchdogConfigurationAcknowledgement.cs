// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// The instance applied a configuration revision atomically.
/// </summary>
/// <param name="Revision">The revision applied.</param>
/// <param name="Digest">The digest of the snapshot applied, which must match the published one.</param>
public record WatchdogConfigurationAcknowledgement(long Revision, string Digest);
