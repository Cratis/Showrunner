// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// A failing check passed again.
/// </summary>
/// <param name="Check">The check's name.</param>
/// <param name="Resource">The resource it watches.</param>
/// <param name="StatusCode">The status received.</param>
/// <param name="ConfigurationRevision">The revision the check ran under.</param>
public record CheckRecovered(string Check, Guid Resource, int StatusCode, long ConfigurationRevision);
