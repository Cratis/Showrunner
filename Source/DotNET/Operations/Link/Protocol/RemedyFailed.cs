// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// A remedy's request failed.
/// </summary>
/// <param name="Remedy">The remedy's name.</param>
/// <param name="Resource">The resource.</param>
/// <param name="StatusCode">The status received - 0 when none arrived.</param>
/// <param name="Reason">What went wrong, without secrets.</param>
public record RemedyFailed(string Remedy, Guid Resource, int StatusCode, string Reason);
