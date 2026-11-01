// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald;

/// <summary>
/// Represents who the emails come from.
/// </summary>
public class EmailSenderOptions
{
    /// <summary>
    /// Gets or sets the address the emails come from. Required.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name shown beside the address.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the address replies go to, when it is not <see cref="Address"/>.
    /// </summary>
    public string ReplyTo { get; set; } = string.Empty;
}
