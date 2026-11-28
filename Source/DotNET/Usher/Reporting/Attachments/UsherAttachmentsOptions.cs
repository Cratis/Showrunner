// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Reporting.Attachments;

/// <summary>
/// Represents how Usher stores the files a reporter attaches to a report.
/// </summary>
public class UsherAttachmentsOptions
{
    /// <summary>
    /// Gets or sets the directory attachments are stored in. Relative to the host's working directory unless the
    /// host sets an absolute, persistent path - a container without a mounted volume loses attachments on redeploy.
    /// Defaults to <c>usher-attachments</c>.
    /// </summary>
    public string Path { get; set; } = "usher-attachments";

    /// <summary>
    /// Gets or sets the largest attachment a reporter can upload, in bytes. Defaults to 10 MiB.
    /// </summary>
    public long MaxSizeInBytes { get; set; } = 10 * 1024 * 1024;
}
