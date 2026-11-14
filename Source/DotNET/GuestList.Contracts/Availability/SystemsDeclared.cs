// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Contracts.Audiences;

namespace Cratis.Showrunner.GuestList.Contracts.Availability;

/// <summary>
/// Published by the product to say which systems call it, so an operator can pick one to grant an audience to, see
/// <see cref="AudienceLevel.NamedSystem"/>. It is the whole list as the product knows it in one environment and replaces the
/// previous one, so a system that is no longer listed is no longer offered. It is reported once per change, and says
/// nothing about who holds which audience.
/// </summary>
/// <param name="Product">The product that reports.</param>
/// <param name="Environment">The environment the product reports from.</param>
/// <param name="Systems">The systems the product knows.</param>
[EventType]
[Public]
public record SystemsDeclared(Product Product, EnvironmentName Environment, IEnumerable<SystemDeclaration> Systems);
