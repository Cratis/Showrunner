// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
namespace Cratis.Showrunner.StageManager.Pulumi.given;

/// <summary>
/// Shared settings of the specifications that run a Pulumi program against mocks.
/// </summary>
public static class PulumiSpecs
{
    /// <summary>
    /// The name of the collection those specifications run in. The Pulumi test deployment is process-wide, so they run one at a time.
    /// </summary>
    public const string Collection = "Pulumi test deployment";

    /// <summary>
    /// The namespace the sample resources are declared in.
    /// </summary>
    public const string Namespace = "keldakraft";
}
#endif
