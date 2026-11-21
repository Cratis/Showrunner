// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// What a policy concludes about a remedy - the part of Direct's assurance vocabulary the offline remedy decision uses.
/// </summary>
public enum AssuranceOutcome
{
    /// <summary>
    /// The remedy may run.
    /// </summary>
    Allow = 0,

    /// <summary>
    /// The remedy must not run.
    /// </summary>
    Block = 1,

    /// <summary>
    /// The remedy needs a person's review.
    /// </summary>
    RequiresHumanReview = 2,
}
