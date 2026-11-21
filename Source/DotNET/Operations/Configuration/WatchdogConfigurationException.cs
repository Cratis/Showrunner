// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Configuration;

/// <summary>
/// Thrown when the environment does not describe a usable watchdog. The message names the variables at fault and never their
/// values, which can be credentials.
/// </summary>
/// <param name="problems">What is wrong.</param>
public sealed class WatchdogConfigurationException(IReadOnlyList<string> problems)
    : Exception("The watchdog is not configured correctly: " + string.Join("; ", problems))
{
    /// <summary>
    /// Gets what is wrong, one entry per problem.
    /// </summary>
    public IReadOnlyList<string> Problems { get; } = problems;
}
