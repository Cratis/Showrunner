// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;

namespace Cratis.Showrunner.Notes.Transport;

/// <summary>
/// The exception that is thrown when the collector answers with anything but success. It carries the status code and
/// nothing of the request or the response, so it can be logged freely.
/// </summary>
/// <param name="statusCode">The status code the collector answered with.</param>
public class CollectorRequestFailed(HttpStatusCode statusCode)
    : Exception($"The collector answered {(int)statusCode} {statusCode}.")
{
    /// <summary>
    /// Gets the status code the collector answered with.
    /// </summary>
    public HttpStatusCode StatusCode { get; } = statusCode;
}
