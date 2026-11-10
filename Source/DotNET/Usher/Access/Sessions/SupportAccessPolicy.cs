// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Identity;

namespace Cratis.Showrunner.Usher.Access.Sessions;

/// <summary>
/// Checks identity, session state and grant in refusal precedence order.
/// </summary>
internal static class SupportAccessPolicy
{
    /// <inheritdoc cref="SupportAccess.RefusalFor"/>
    internal static SupportSessionRefusal? RefusalFor(SupportAccess access, SupportSessionRequest request, DateTimeOffset now) =>
        IdentityRefusal(access, request) ?? SessionRefusal(access, request.SessionId) ?? GrantRefusal(access, now);

    static SupportSessionRefusal? IdentityRefusal(SupportAccess access, SupportSessionRequest request)
    {
        if (!access.IsReported)
        {
            return SupportSessionRefusal.IssueNotReported;
        }

        if (access.ReportedBy.Subject != request.Subject)
        {
            return SupportSessionRefusal.NotTheReporter;
        }

        // The same subject at another identity provider is somebody else; only the reporter's own identity is them.
        return request.Actor == access.ReportedBy ? SupportSessionRefusal.ActorIsTheReporter : null;
    }

    static SupportSessionRefusal? SessionRefusal(SupportAccess access, SupportSessionId sessionId)
    {
        if (access.HasEnded(sessionId))
        {
            return SupportSessionRefusal.SessionEnded;
        }

        return access.Withdrawn ? SupportSessionRefusal.IssueWithdrawn : null;
    }

    static SupportSessionRefusal? GrantRefusal(SupportAccess access, DateTimeOffset now)
    {
        if (!access.HasLiveGrant(now))
        {
            return SupportSessionRefusal.NoLiveGrant;
        }

        return access.SharedIdentity is not null && UserIdentity.Of(access.SharedIdentity) == access.ReportedBy ? null : SupportSessionRefusal.IdentityNotShared;
    }
}
