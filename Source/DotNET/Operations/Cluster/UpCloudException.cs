// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// Thrown when the UpCloud API refuses a request. The message never carries the response body.
/// </summary>
/// <param name="message">The method, path and HTTP status.</param>
/// <param name="code">The error code UpCloud answered with, when it did.</param>
public sealed class UpCloudException(string message, string? code) : Exception(message)
{
    /// <summary>
    /// Gets the error code UpCloud answered with.
    /// </summary>
    public string? Code { get; } = code;
}
