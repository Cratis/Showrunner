// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// Defines the on-behalf session the current request was forwarded in, as AuthProxy's verified assertion states it.
/// </summary>
public interface IOnBehalfForward
{
    /// <summary>
    /// Gets what AuthProxy's verified assertion says about the session of the current request, and whether the session
    /// is live; <see langword="null"/> when the request was not forwarded on behalf of anybody, or its assertion was not
    /// verified.
    /// </summary>
    VerifiedForward? Current { get; }
}
