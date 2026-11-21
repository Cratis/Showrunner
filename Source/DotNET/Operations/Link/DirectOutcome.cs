// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// How a request to Direct came out - each is one HTTP status the link acts on.
/// </summary>
public enum DirectOutcome
{
    /// <summary>
    /// <c>200 OK</c>.
    /// </summary>
    Ok = 0,

    /// <summary>
    /// <c>204 No Content</c> - nothing newer.
    /// </summary>
    NoContent = 1,

    /// <summary>
    /// <c>400 Bad Request</c> - do not retry the request unchanged.
    /// </summary>
    Rejected = 2,

    /// <summary>
    /// <c>401 Unauthorized</c> - the credential is not accepted.
    /// </summary>
    Unauthorized = 3,

    /// <summary>
    /// <c>409 Conflict</c> - fenced: another session holds the instance, or the epoch is stale. Stop acting.
    /// </summary>
    Fenced = 4,

    /// <summary>
    /// Direct could not be reached, answered <c>503</c> or something unexpected - retry later.
    /// </summary>
    Unavailable = 5,
}
