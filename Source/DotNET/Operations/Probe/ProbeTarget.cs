// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Probe;

/// <summary>
/// A public site the synthetic probe calls.
/// </summary>
/// <param name="Name">The name of the target.</param>
/// <param name="BaseUrl">The base address, without a trailing path.</param>
/// <param name="Checks">The requests to make.</param>
/// <param name="Note">What a reader should know about the target; it is not part of any alert.</param>
public sealed record ProbeTarget(string Name, string BaseUrl, IReadOnlyList<ProbeEndpoint> Checks, string? Note = null);
