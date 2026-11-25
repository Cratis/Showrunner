// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Contracts.Suspensions;

namespace Cratis.Showrunner.GuestList.Contracts.Availability;

/// <summary>
/// One part of an application an operator can suspend: a module, a feature, a slice, or a command, query or screen.
/// </summary>
/// <param name="Kind">What the part is: <c>module</c>, <c>feature</c>, <c>slice</c>, <c>command</c>, <c>query</c> or <c>screen</c>.</param>
/// <param name="Path">The path the operator suspends the part by; a path covers everything below it.</param>
/// <param name="Name">The name the part has in the application.</param>
public record PartDeclaration(string Kind, PartPath Path, string Name);
