// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// A call AuthProxy made to the application, as the application received it: what its token must be bound to.
/// </summary>
/// <param name="Purpose">The <c>purpose</c> the endpoint expects.</param>
/// <param name="Method">The HTTP method.</param>
/// <param name="PathBase">The path base the application is hosted on.</param>
/// <param name="Path">The path of the endpoint, below the path base.</param>
/// <param name="Body">The exact body, as read before deserializing; empty for the grant check.</param>
public record OnBehalfCall(string Purpose, string Method, string PathBase, string Path, ReadOnlyMemory<byte> Body);
