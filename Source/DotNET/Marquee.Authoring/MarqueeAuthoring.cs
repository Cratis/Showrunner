// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Authoring;

/// <summary>
/// What the host needs to know about Marquee's authoring slices.
/// </summary>
public static class MarqueeAuthoring
{
    /// <summary>
    /// The name of the authorization policy every authoring command and the authoring query require. Authorization is
    /// the host's: it registers a policy of this name, for example one that admits its administrators, either as an
    /// ASP.NET Core authorization policy or with <c>AddArcAuthorizationPolicy</c>. A host that does not register it can
    /// not use the authoring commands.
    /// </summary>
    public const string Policy = "Cratis.Showrunner.Marquee.Authoring";
}
