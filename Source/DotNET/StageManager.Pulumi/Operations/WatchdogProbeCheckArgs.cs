// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// One request of a <see cref="WatchdogProbeTargetArgs"/> and the status it must answer with.
/// </summary>
public sealed class WatchdogProbeCheckArgs
{
    /// <summary>
    /// Gets the id of the check, unique within its target.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the path, beginning with a slash.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Gets the HTTP status the endpoint must answer with. An anonymous 401 is how an endpoint behind an authenticating proxy
    /// proves the proxy answers.
    /// </summary>
    public required int ExpectStatus { get; init; }

    /// <summary>
    /// Gets the HTTP method, or null for <c>GET</c>.
    /// </summary>
    public string? Method { get; init; }

    /// <summary>
    /// Gets the Accept header, or null for <c>application/json</c>.
    /// </summary>
    public string? Accept { get; init; }

    /// <summary>
    /// Gets the JSON body, or null for none.
    /// </summary>
    public string? Body { get; init; }

    /// <summary>
    /// Gets how long to wait for an answer in milliseconds, or null for ten seconds.
    /// </summary>
    public int? TimeoutMilliseconds { get; init; }
}
