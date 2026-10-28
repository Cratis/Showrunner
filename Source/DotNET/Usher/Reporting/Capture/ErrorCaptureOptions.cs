// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Reporting.Capture;

/// <summary>
/// Represents how Usher captures the errors of failing commands.
/// </summary>
public class ErrorCaptureOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether failing commands are captured. Defaults to <see langword="true"/>.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets how long a captured error is kept for a report to use. Defaults to 15 minutes.
    /// </summary>
    public TimeSpan Retention { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Gets or sets how many captured errors are kept at most, across all tenants and users; the oldest go first.
    /// Defaults to 1000.
    /// </summary>
    public int Capacity { get; set; } = 1000;

    /// <summary>
    /// Gets or sets how many stack frames, from the top, are kept of a captured error. Defaults to 50.
    /// </summary>
    public int MaximumStackFrames { get; set; } = 50;
}
