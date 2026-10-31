// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Metering;

/// <summary>
/// Options for measuring compute.
/// </summary>
public class MeteringOptions
{
    /// <summary>
    /// Gets or sets how long after midnight UTC a day closes and is reported. Usage that arrives later is a correction. Defaults to 6 hours.
    /// </summary>
    public TimeSpan CloseDelay { get; set; } = TimeSpan.FromHours(6);

    /// <summary>
    /// Gets or sets how often tenants are measured. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets how long an instance holds the right to measure without renewing it. Defaults to 2 minutes.
    /// </summary>
    public TimeSpan Lease { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Gets or sets the name of the MongoDB collection that keeps the buckets and the metering state. Defaults to <c>boxoffice_metering</c>.
    /// </summary>
    public string Collection { get; set; } = "boxoffice_metering";
}
