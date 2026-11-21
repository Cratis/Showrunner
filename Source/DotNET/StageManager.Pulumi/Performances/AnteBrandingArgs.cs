// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// How Ante looks: the logo, the stylesheet and the loading screen. Every value is optional.
/// </summary>
public sealed class AnteBrandingArgs
{
    /// <summary>
    /// Gets the logo: a path on the host of Ante, such as <c>/brand/logo.svg</c>, or an https URL.
    /// </summary>
    public string? LogoUrl { get; init; }

    /// <summary>
    /// Gets the stylesheet: a path on the host of Ante, such as <c>/brand/lobby.css</c>, or an https URL.
    /// </summary>
    public string? CustomCssUrl { get; init; }

    /// <summary>
    /// Gets the browser tab title.
    /// </summary>
    public string? PageTitle { get; init; }

    /// <summary>
    /// Gets the text announced while Ante loads.
    /// </summary>
    public string? SplashMessage { get; init; }

    /// <summary>
    /// Gets the files Ante serves under <c>/brand</c>.
    /// </summary>
    public AnteBrandingAssetsArgs? Assets { get; init; }

    /// <summary>
    /// Builds the spec of the branding of Ante, declaring what it refers to.
    /// </summary>
    /// <param name="context">The context of the Performance.</param>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec(PerformanceContext context) =>
        SpecExtensions.NewSpec()
            .Put("logoUrl", LogoUrl)
            .Put("customCssUrl", CustomCssUrl)
            .Put("pageTitle", PageTitle)
            .Put("splashMessage", SplashMessage)
            .PutObject("assets", Assets?.ToSpec(context) ?? SpecExtensions.NewSpec());
}
