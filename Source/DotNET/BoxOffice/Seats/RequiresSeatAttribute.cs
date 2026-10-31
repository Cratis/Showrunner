// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Seats;

/// <summary>
/// Marks a command that only a user who holds a seat may execute. The product says which commands those are; Box Office
/// refuses the command from a user without a seat with <c>SeatRequired</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RequiresSeatAttribute : Attribute;
