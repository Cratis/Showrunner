// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Trials;

/// <summary>
/// How a trial came to an end.
/// </summary>
public enum TrialClosure
{
    /// <summary>
    /// The trial ran to its end date.
    /// </summary>
    Expired = 0,

    /// <summary>
    /// An administrator ended the trial before its end date.
    /// </summary>
    EndedEarly = 1,

    /// <summary>
    /// The organization became a customer.
    /// </summary>
    Converted = 2
}
