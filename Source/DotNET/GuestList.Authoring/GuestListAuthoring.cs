// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Authoring;

/// <summary>
/// What the host needs to know about GuestList's authoring slices.
/// </summary>
public static class GuestListAuthoring
{
    /// <summary>
    /// The name of the authorization policy every authoring command and query require. Authorization is the host's: it
    /// registers a policy of this name, for example one that admits its administrators, either as an ASP.NET Core
    /// authorization policy or with <c>AddArcAuthorizationPolicy</c>. A host that does not register it can not use the
    /// authoring commands.
    /// </summary>
    public const string Policy = "Cratis.Showrunner.GuestList.Authoring";
}
