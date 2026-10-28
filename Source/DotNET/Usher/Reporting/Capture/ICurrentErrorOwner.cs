// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace Cratis.Showrunner.Usher.Reporting.Capture;

/// <summary>
/// Defines a system that knows whose errors the current request may capture and use.
/// </summary>
public interface ICurrentErrorOwner
{
    /// <summary>
    /// Gets the <see cref="ErrorOwner"/> of the current request: the signed-in user in the current tenant.
    /// </summary>
    /// <param name="owner">The <see cref="ErrorOwner"/>, when someone is signed in.</param>
    /// <returns>True when someone is signed in; errors of anonymous requests are never captured.</returns>
    bool TryGet([NotNullWhen(true)] out ErrorOwner? owner);
}
