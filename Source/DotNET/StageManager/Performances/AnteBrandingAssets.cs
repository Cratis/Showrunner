// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Static files, such as a logo and a stylesheet, Ante serves itself so they need no separate host.
/// </summary>
public class AnteBrandingAssets
{
    /// <summary>
    /// Where the files are mounted in Ante's web root.
    /// </summary>
    public const string MountDirectory = "/app/wwwroot/brand";

    /// <summary>
    /// Gets or sets the ConfigMap holding the files.
    /// </summary>
    [Required]
    [Description("The ConfigMap holding the files, one per key. Ante serves each of them under /brand/KEY.")]
    public ConfigMapKeyReference ConfigMapRef { get; set; } = new();

    /// <summary>
    /// Gets or sets the keys of the ConfigMap to serve.
    /// </summary>
    [Required]
    [Description("The keys of the ConfigMap to serve, each mounted as its own file: a mounted folder would expose symbolic links, which Ante's file server reports with the wrong length.")]
    public IList<string> Files { get; set; } = [];
}
