// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Marquee.Authoring.given;

/// <summary>
/// A command scenario for a command on an announcement, run as an operator the host's authoring policy admits unless the
/// specification says otherwise.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
public class an_announcement_command<TCommand> : Specification
    where TCommand : class
{
    protected CommandScenario<TCommand> _scenario;
    protected AnnouncementId _announcementId;
    protected CommandResult _result;

    /// <summary>
    /// Gets the principal the command runs as: an operator, signed in.
    /// </summary>
    protected virtual ClaimsPrincipal Principal => Operators.Operator;

    void Establish()
    {
        _announcementId = AnnouncementId.New();
        _scenario = new();
        _scenario.Services.AddSingleton<ICurrentPrincipalAccessor>(new FixedPrincipal(Principal));
        _scenario.Services.AddArcAuthorizationPolicy<AdmitOperators>(MarqueeAuthoring.Policy);
    }

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Seeds events on the announcement before the command runs.
    /// </summary>
    /// <param name="events">The events, in the order they happened; arrays of events are flattened.</param>
    protected void Given(params object[] events) => _scenario.Given.ForEventSource(_announcementId).Events(AnnouncementEvents.Then(events));
}

/// <summary>
/// An <see cref="ICurrentPrincipalAccessor"/> that gives a fixed principal.
/// </summary>
/// <param name="principal">The principal.</param>
public class FixedPrincipal(ClaimsPrincipal principal) : ICurrentPrincipalAccessor
{
    /// <inheritdoc/>
    public ClaimsPrincipal? Current => principal;
}
#endif
