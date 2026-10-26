// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Identity;

namespace Cratis.Showrunner.Usher.Access.Sessions;

/// <summary>
/// Support asking to act as a reporter on one issue, in one session.
/// </summary>
/// <param name="IssueId">The issue the grant is for.</param>
/// <param name="SessionId">The session.</param>
/// <param name="Subject">The subject of the user support wants to act as.</param>
/// <param name="Actor">The subject of the support person or agent who acts.</param>
public record SupportSessionRequest(IssueId IssueId, SupportSessionId SessionId, IdentityProviderSubject Subject, IdentityProviderSubject Actor);
