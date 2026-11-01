// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager;

/// <summary>
/// The Kubernetes API group and version of the Stage Manager custom resources.
/// </summary>
public static class StageManagerApi
{
    /// <summary>
    /// The API group of every Stage Manager resource.
    /// </summary>
    public const string Group = "showrunner.cratis.io";

    /// <summary>
    /// The API version of every Stage Manager resource.
    /// </summary>
    public const string Version = "v1alpha1";
}
