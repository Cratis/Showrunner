// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The custom pages AuthProxy shows instead of its built-in ones.
/// </summary>
public class AuthProxyPagesSpec
{
    /// <summary>
    /// Gets or sets the ConfigMap holding the pages.
    /// </summary>
    [Required]
    [Description("The ConfigMap holding the pages and their assets, one file per key: 403.html, 404.html, select-tenant.html, styles.css and so on. A page that is present replaces the built-in one; the rest keep AuthProxy's own.")]
    public ConfigMapKeyReference ConfigMapRef { get; set; } = new();
}
