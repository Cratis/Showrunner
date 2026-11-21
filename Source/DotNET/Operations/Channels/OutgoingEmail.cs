// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Channels;

/// <summary>
/// A plain-text email.
/// </summary>
/// <param name="FromName">The display name of the sender.</param>
/// <param name="From">The sender address.</param>
/// <param name="To">The recipient address, or several separated by commas.</param>
/// <param name="Subject">The subject.</param>
/// <param name="Body">The plain-text body.</param>
public sealed record OutgoingEmail(string FromName, string From, string To, string Subject, string Body);
