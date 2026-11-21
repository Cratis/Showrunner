// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// A check did not get an expected answer.
/// </summary>
/// <param name="Check">The check's name.</param>
/// <param name="Resource">The resource it watches.</param>
/// <param name="StatusCode">The status received - 0 when no response arrived.</param>
/// <param name="Reason">What went wrong, without secrets.</param>
/// <param name="ConsecutiveFailures">How many runs in a row have failed, this one included - Direct raises an alert from the second.</param>
/// <param name="ConfigurationRevision">The revision the check ran under.</param>
public record CheckFailed(string Check, Guid Resource, int StatusCode, string Reason, int ConsecutiveFailures, long ConfigurationRevision);
