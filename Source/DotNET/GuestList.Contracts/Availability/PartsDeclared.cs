// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Contracts.Audiences;

namespace Cratis.Showrunner.GuestList.Contracts.Availability;

/// <summary>
/// Published by a product to tell the operator every part of its application, so an operator can pick one to suspend. It is
/// the whole list, not a change to it: the newest report of an environment replaces the one before. A product reports when
/// the parts it has changed, and it reports every module, feature, slice, command, query and screen, declared in the model
/// or not, because any part can be suspended.
/// </summary>
/// <param name="Product">The product that reports.</param>
/// <param name="Environment">The environment the product runs in.</param>
/// <param name="Parts">Every part of the application.</param>
[EventType]
[Public]
public record PartsDeclared(Product Product, EnvironmentName Environment, IEnumerable<PartDeclaration> Parts);
