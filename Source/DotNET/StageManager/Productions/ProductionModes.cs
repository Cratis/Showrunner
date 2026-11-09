// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// How a dependency of a <see cref="Production"/>, its database or its graph, is provided.
/// </summary>
public static class ProductionModes
{
    /// <summary>
    /// Stage Manager runs it for the Production.
    /// </summary>
    public const string Managed = "Managed";

    /// <summary>
    /// It runs elsewhere and the Production reaches it through a Secret.
    /// </summary>
    public const string External = "External";

    /// <summary>
    /// The Production has none.
    /// </summary>
    public const string None = "None";
}
