// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Gating;

/// <summary>
/// Why a command or a query is refused for want of a module. Constants, so a client and a specification can match the
/// reason without matching presentation text.
/// </summary>
public static class GatingReasons
{
    /// <summary>
    /// The tenant does not have the module the command or the query belongs to: it was never granted, the grant was
    /// revoked, the trial or grant ended, or the module is only open as a preview.
    /// </summary>
    public const string ModuleNotEntitled = "ModuleNotEntitled";
}
