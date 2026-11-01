// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Resources;

namespace Cratis.Showrunner.StageManager.BackstagePasses;

/// <summary>
/// The observed state of a <see cref="BackstagePass"/>. Schema only: nothing reconciles it yet.
/// </summary>
public class BackstagePassStatus
{
    /// <summary>
    /// Gets or sets the conditions that describe the resource.
    /// </summary>
    [Description("The conditions that describe the resource.")]
    public IList<ResourceCondition> Conditions { get; set; } = [];
}
