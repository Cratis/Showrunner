// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Suspension;

/// <summary>
/// Marks a command that a suspended tenant, or one whose trial closed without seats, may still execute. Use it for what a
/// read-only tenant must still be able to do, such as reporting a problem to support, exporting its data or leaving.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class AllowedWhileSuspendedAttribute : Attribute;
