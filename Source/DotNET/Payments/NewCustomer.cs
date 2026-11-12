// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Payments;

/// <summary>
/// A customer to register with the provider.
/// </summary>
/// <param name="Name">The customer's name.</param>
/// <param name="Email">The customer's email address, when known.</param>
/// <param name="Metadata">What to keep on the customer.</param>
public record NewCustomer(string Name, string? Email, IReadOnlyDictionary<string, string> Metadata);
