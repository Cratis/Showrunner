// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.BackstagePasses;

/// <summary>
/// What a <see cref="BackstagePass"/> asks for. Schema only: the reconciliation that acts on it is a separate piece of work.
/// </summary>
public class BackstagePassSpec
{
    /// <summary>
    /// Gets or sets the service accounts that may use the API, as <c>namespace/name</c>.
    /// </summary>
    [Description("The service accounts that may use the API, as namespace/name.")]
    public IList<string> ServiceAccounts { get; set; } = [];

    /// <summary>
    /// Gets or sets what the pass allows: <c>Read</c>, <c>Download</c> or <c>Restore</c>.
    /// </summary>
    [Description("What the pass allows: Read, Download or Restore.")]
    public IList<string> Permissions { get; set; } = [];
}
