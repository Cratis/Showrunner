// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Reporting;

/// <summary>
/// Where in the application an issue happened, as the module, feature and slice Arc resolves - on the backend
/// from the executing command or query, in the frontend from the component hierarchy. A part Arc cannot resolve
/// is <c>NotSet</c>, never absent.
/// </summary>
/// <param name="Module">The module.</param>
/// <param name="Feature">The feature within the module.</param>
/// <param name="Slice">The slice within the feature.</param>
public record SliceLocation(ModuleName Module, FeatureName Feature, SliceName Slice)
{
    /// <summary>
    /// Represents a location that could not be resolved: every part is <c>NotSet</c>.
    /// </summary>
    public static readonly SliceLocation NotSet = new(ModuleName.NotSet, FeatureName.NotSet, SliceName.NotSet);
}
