// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald;

/// <summary>
/// Represents how the connection to the mail server is secured.
/// </summary>
public enum SmtpSecurity
{
    /// <summary>
    /// The connection starts in the clear and is upgraded with STARTTLS, usually on port 587. The upgrade is required.
    /// </summary>
    StartTls = 0,

    /// <summary>
    /// The connection is TLS from the first byte, usually on port 465.
    /// </summary>
    Tls = 1,

    /// <summary>
    /// The connection is not secured. Only for a relay on the same machine or network.
    /// </summary>
    None = 2
}
