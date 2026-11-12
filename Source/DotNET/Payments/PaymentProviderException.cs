// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Payments;

/// <summary>
/// The provider refused a request or could not be reached.
/// </summary>
/// <param name="operation">The operation that failed.</param>
/// <param name="statusCode">The provider's HTTP status code, or zero when there was no answer.</param>
/// <param name="body">What the provider answered.</param>
public sealed class PaymentProviderException(string operation, int statusCode, string body)
    : Exception($"Payment provider request '{operation}' failed with status {statusCode}: {body}")
{
    /// <summary>
    /// Gets the operation that failed.
    /// </summary>
    public string Operation { get; } = operation;

    /// <summary>
    /// Gets the provider's HTTP status code, or zero when there was no answer.
    /// </summary>
    public int StatusCode { get; } = statusCode;

    /// <summary>
    /// Gets what the provider answered.
    /// </summary>
    public string Body { get; } = body;
}
