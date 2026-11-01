// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald;

/// <summary>
/// Represents the mail server emails are sent to.
/// </summary>
public class SmtpOptions
{
    /// <summary>
    /// Gets or sets the server. Required for <see cref="EmailTransportKind.Smtp"/>.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the port. Defaults to 587.
    /// </summary>
    public int Port { get; set; } = 587;

    /// <summary>
    /// Gets or sets how the connection is secured. Defaults to <see cref="SmtpSecurity.StartTls"/>.
    /// </summary>
    public SmtpSecurity Security { get; set; } = SmtpSecurity.StartTls;

    /// <summary>
    /// Gets or sets the user name. Leave empty for a server that does not ask who is sending.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password. It is a secret: supply it from the environment or a secret store, never from a file in
    /// the repository. Herald uses it to sign in and nowhere else; it is never logged, put in an event or shown in an
    /// error.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Returns a description that leaves the password out.
    /// </summary>
    /// <returns>The server, port and security.</returns>
    public override string ToString() => $"{Host}:{Port} ({Security})";
}
