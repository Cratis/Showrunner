// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi;

/// <summary>
/// The API group and version of the Stage Manager custom resources, as the operator's chart installs them.
/// </summary>
public static class ShowrunnerApi
{
    /// <summary>
    /// The API group of the custom resources.
    /// </summary>
    public const string Group = "showrunner.cratis.io";

    /// <summary>
    /// The version of the custom resources.
    /// </summary>
    public const string Version = "v1alpha1";

    /// <summary>
    /// The <c>apiVersion</c> of the custom resources.
    /// </summary>
    public const string ApiVersion = $"{Group}/{Version}";
}
