// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// What running one of Direct's endpoint checks found.
/// </summary>
/// <param name="Name">The check's name.</param>
/// <param name="Resource">The resource it watches.</param>
/// <param name="Passed">Whether the status was in the expected range.</param>
/// <param name="StatusCode">The status received, zero when none arrived.</param>
/// <param name="Reason">Why it failed, without secrets.</param>
public sealed record EndpointCheckResult(string Name, Guid Resource, bool Passed, int StatusCode, string Reason);
