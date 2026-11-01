// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald.Rendering;

/// <summary>
/// Represents a value an email accepts.
/// </summary>
/// <param name="Name">The name of the value as the template writes it, such as <c>invitation.link</c>.</param>
/// <param name="Required">Whether the email can not be sent without it.</param>
public record EmailValueDeclaration(string Name, bool Required);
