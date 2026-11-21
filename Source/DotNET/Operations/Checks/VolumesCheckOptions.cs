// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// The options of <see cref="VolumesCheck"/>.
/// </summary>
public sealed record VolumesCheckOptions
{
    /// <summary>
    /// Gets the usage, in percent, above which a volume is a finding. It is capped at 85.
    /// </summary>
    public int ThresholdPercent { get; init; } = 85;

    /// <summary>
    /// Gets what the finding tells the reader to do, for example a link to the deployment's storage recovery runbook.
    /// </summary>
    public string Advice { get; init; } = "expand the volume or free space on it";
}
