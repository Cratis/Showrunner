// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// How a remedy went.
/// </summary>
/// <param name="Succeeded">Whether the remedy's action was accepted. Whether it <em>worked</em> is only known when its verification passes.</param>
/// <param name="StatusCode">The HTTP status for an HTTP remedy, zero otherwise.</param>
/// <param name="Detail">What happened, without secrets.</param>
public sealed record RemedyResult(bool Succeeded, int StatusCode, string Detail);
