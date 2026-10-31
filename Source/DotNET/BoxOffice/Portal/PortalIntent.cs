// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Portal;

/// <summary>
/// What the person who opens the customer portal wants to do there.
/// </summary>
public enum PortalIntent
{
    /// <summary>
    /// Buy something the operator sells.
    /// </summary>
    Buy = 0,

    /// <summary>
    /// Try something the operator offers a trial of.
    /// </summary>
    Try = 1,

    /// <summary>
    /// Move to another plan.
    /// </summary>
    ChangePlan = 2
}
