// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Access.Sessions;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// A request forwarded in an on-behalf session that AuthProxy's assertion vouched for, with what the issue's own events
/// say about the session at the moment the request arrived.
/// </summary>
/// <remarks>
/// AuthProxy rechecks a grant only every so often. The issue's events are read for every forwarded request, so a
/// grant revoked, lapsed or ended by withdrawing the issue, or a session that ended, keeps support from acting in Usher
/// from the moment it is recorded.
/// </remarks>
/// <param name="Assertion">The verified <see cref="OnBehalfAssertion"/>.</param>
/// <param name="Refusal">Why support may not act in the session now; <see langword="null"/> when it may.</param>
public record VerifiedForward(OnBehalfAssertion Assertion, SupportSessionRefusal? Refusal)
{
    /// <summary>
    /// Gets whether support may act in the session now: the grant is live and the session has not ended.
    /// </summary>
    public bool IsLive => Refusal is null;
}
