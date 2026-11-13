// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Contracts.Availability;

/// <summary>
/// A value the conditions of the model read.
/// </summary>
/// <param name="Name">The name of the value.</param>
/// <param name="Type">Its type; empty when unknown.</param>
/// <param name="Origin">Where it comes from, such as the provider that supplies it.</param>
/// <param name="Scope">What it describes, as a dotted path from the application down; empty for the whole application.</param>
/// <param name="Required">Whether a gate that reads it stays closed while it is unknown.</param>
public record ContextValueDeclaration(string Name, string Type, string Origin, string Scope, bool Required);
