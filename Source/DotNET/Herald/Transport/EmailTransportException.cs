// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald.Transport;

/// <summary>
/// The exception that is thrown when a transport did not accept a message.
/// </summary>
/// <param name="message">What went wrong, without a credential or the message content.</param>
/// <param name="permanent">Whether sending again would fail the same way: the server refused this message or recipient for good.</param>
/// <param name="innerException">The cause.</param>
public class EmailTransportException(string message, bool permanent, Exception? innerException = null) : Exception(message, innerException)
{
    /// <summary>
    /// Gets a value indicating whether sending again would fail the same way. A permanent failure is not retried.
    /// </summary>
    public bool Permanent { get; } = permanent;
}
