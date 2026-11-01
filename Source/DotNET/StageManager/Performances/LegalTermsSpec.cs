// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Where the legal terms of an application are.
/// </summary>
public class LegalTermsSpec
{
    /// <summary>
    /// Gets or sets the ConfigMap holding the terms and privacy policy as files (terms.md and privacy.md).
    /// </summary>
    [Required]
    [Description("The ConfigMap holding the terms and privacy policy files: terms.md and privacy.md.")]
    public ConfigMapKeyReference ConfigMapRef { get; set; } = new();
}
