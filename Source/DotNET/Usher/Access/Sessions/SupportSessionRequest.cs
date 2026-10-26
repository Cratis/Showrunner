// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Identity;
using Cratis.Showrunner.Usher.Identity;

namespace Cratis.Showrunner.Usher.Access.Sessions;

/// <summary>
/// Support asking to act as a reporter on one issue, in one session.
/// </summary>
/// <param name="IssueId">The issue the grant is for.</param>
/// <param name="SessionId">The session.</param>
/// <param name="Subject">
/// The subject of the user support wants to act as. AuthProxy's token names the subject only; the issue names the
/// identity provider, and the grant check answers with the reporter's full identity, which is what AuthProxy forwards.
/// </param>
/// <param name="Actor">Who the support person or agent who acts is, at their identity provider.</param>
public record SupportSessionRequest(IssueId IssueId, SupportSessionId SessionId, IdentityProviderSubject Subject, UserIdentity Actor);
