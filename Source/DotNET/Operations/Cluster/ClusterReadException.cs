// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// Thrown when the cluster cannot be read.
/// </summary>
/// <param name="message">What could not be read.</param>
/// <param name="statusCode">The HTTP status the API answered with, when it answered.</param>
/// <param name="innerException">The underlying exception.</param>
public sealed class ClusterReadException(string message, int? statusCode = null, Exception? innerException = null) : Exception(message, innerException)
{
    /// <summary>
    /// Gets the HTTP status the API answered with, when it answered.
    /// </summary>
    public int? StatusCode { get; } = statusCode;
}
