// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Alerts;

/// <summary>
/// How serious a finding is. Direct reads the same three levels.
/// </summary>
public enum AlertSeverity
{
    /// <summary>
    /// Worth knowing, nothing is broken.
    /// </summary>
    Information = 0,

    /// <summary>
    /// Something is degrading and needs attention soon.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Something is broken or about to be.
    /// </summary>
    Critical = 2
}
