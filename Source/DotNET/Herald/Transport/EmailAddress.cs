// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald.Transport;

/// <summary>
/// Represents the address of a sender or recipient.
/// </summary>
/// <param name="Address">The address, such as <c>ada@example.com</c>.</param>
/// <param name="Name">The name shown beside it; empty for none.</param>
public record EmailAddress(string Address, string Name = "");
