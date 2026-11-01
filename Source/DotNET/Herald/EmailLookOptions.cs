// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald;

/// <summary>
/// Represents the look shared by every template. Templates use it as <c>{{look.brandColor}}</c> and
/// <c>{{look.footer}}</c> without declaring them.
/// </summary>
public class EmailLookOptions
{
    /// <summary>
    /// Gets or sets the brand color, as <c>#rrggbb</c>, for buttons and links. Defaults to a blue.
    /// </summary>
    public string BrandColor { get; set; } = "#2563eb";

    /// <summary>
    /// Gets or sets the footer text: the company, its address, a way to get help. It is plain text and is encoded like any
    /// other value.
    /// </summary>
    public string Footer { get; set; } = string.Empty;
}
