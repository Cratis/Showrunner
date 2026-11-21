// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Channels;

/// <summary>
/// The SMTP server and addresses alerts are mailed with. Connection is STARTTLS, then login.
/// </summary>
/// <param name="Server">The host name of the SMTP server.</param>
/// <param name="Port">The port, usually 587.</param>
/// <param name="Username">The user to log in as.</param>
/// <param name="Password">The password. It is never logged.</param>
/// <param name="FromName">The display name of the sender.</param>
/// <param name="From">The sender address.</param>
/// <param name="To">The recipient address, or several separated by commas.</param>
public sealed record SmtpOptions(string Server, int Port, string Username, string Password, string FromName, string From, string To);
