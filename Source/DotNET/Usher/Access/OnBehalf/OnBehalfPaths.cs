// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// The paths, purposes and headers of AuthProxy's on-behalf calls to the application (Cratis/AuthProxy#217).
/// </summary>
public static class OnBehalfPaths
{
    /// <summary>
    /// The path of the grant check.
    /// </summary>
    public const string Grants = "/.cratis/on-behalf/grants";

    /// <summary>
    /// The path of the notification that a session started.
    /// </summary>
    public const string SessionStarted = "/.cratis/on-behalf/sessions/started";

    /// <summary>
    /// The path of the notification that a session ended.
    /// </summary>
    public const string SessionEnded = "/.cratis/on-behalf/sessions/ended";

    /// <summary>
    /// The <c>purpose</c> of the token on a grant check.
    /// </summary>
    public const string GrantCheckPurpose = "on-behalf-grant-check";

    /// <summary>
    /// The <c>purpose</c> of the token on the notification that a session started.
    /// </summary>
    public const string SessionStartedPurpose = "on-behalf-session-started";

    /// <summary>
    /// The <c>purpose</c> of the token on the notification that a session ended.
    /// </summary>
    public const string SessionEndedPurpose = "on-behalf-session-ended";

    /// <summary>
    /// The <c>purpose</c> of the assertion forwarded beside each request of a session.
    /// </summary>
    public const string ForwardPurpose = "on-behalf-forward";

    /// <summary>
    /// The prefix of every path AuthProxy calls for on-behalf sessions.
    /// </summary>
    public const string Prefix = "/.cratis/on-behalf";

    /// <summary>
    /// The header carrying the actor of a session.
    /// </summary>
    public const string ActorHeader = "x-cratis-on-behalf-actor";

    /// <summary>
    /// The header carrying the assertion forwarded beside each request of a session.
    /// </summary>
    public const string AssertionHeader = "x-cratis-on-behalf-assertion";

    /// <summary>
    /// The header carrying the represented user of a session.
    /// </summary>
    public const string PrincipalHeader = "x-ms-client-principal";

    /// <summary>
    /// The header naming the tenant of the session.
    /// </summary>
    public const string TenantHeader = "x-cratis-tenant-id";
}
