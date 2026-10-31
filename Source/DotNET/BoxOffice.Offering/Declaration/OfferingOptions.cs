// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration;

/// <summary>
/// Options for reporting the offering.
/// </summary>
public class OfferingOptions
{
    /// <summary>
    /// Gets or sets how long to wait before trying again when the offering could not be reported, for example
    /// because Chronicle is not up yet. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(30);
}
