// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies;

/// <summary>
/// What the operator knows about Understudies that is not in a resource.
/// </summary>
/// <param name="DefaultImage">The image of the backup job when the Understudy names none: the Understudy image of the operator's own release.</param>
public record UnderstudyOptions(string DefaultImage)
{
    /// <summary>
    /// The image used when neither the Understudy nor the operator's configuration names one.
    /// </summary>
    public const string FallbackImage = "cratis/showrunner-understudy-image:latest";

    /// <summary>
    /// The configuration key that carries the default image. In the operator's environment it is <c>Understudy__Image</c>.
    /// </summary>
    public const string ImageKey = "Understudy:Image";
}
