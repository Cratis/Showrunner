// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// How Ante looks: the logo, the stylesheet and the loading screen. Every value is optional; an omitted one keeps Ante's own.
/// </summary>
public class AnteBrandingSpec
{
    /// <summary>
    /// Gets or sets the logo address.
    /// </summary>
    [Description("The logo: a path on the host of Ante, such as /brand/logo.svg, or an https URL. Maps to Ante:LogoUrl.")]
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Gets or sets the stylesheet address.
    /// </summary>
    [Description("The stylesheet: a path on the host of Ante, such as /brand/lobby.css, or an https URL. Maps to Ante:CustomCssUrl.")]
    public string? CustomCssUrl { get; set; }

    /// <summary>
    /// Gets or sets the browser tab title.
    /// </summary>
    [Description("The browser tab title. Maps to Ante:PageTitle.")]
    public string? PageTitle { get; set; }

    /// <summary>
    /// Gets or sets the text announced while Ante loads.
    /// </summary>
    [Description("The text assistive technology announces on the loading screen. Maps to Ante:SplashMessage.")]
    public string? SplashMessage { get; set; }

    /// <summary>
    /// Gets or sets the files Ante serves itself.
    /// </summary>
    [Description("Files Ante serves under /brand, so the logo and the stylesheet can be local to the cluster.")]
    public AnteBrandingAssets? Assets { get; set; }
}
