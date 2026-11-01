// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald.Rendering;

/// <summary>
/// The exception that is thrown when an email can not be rendered: its template is missing or does not follow the rules,
/// a value is unknown or missing, or an image is not available. Nothing is sent for such an email.
/// </summary>
/// <param name="message">What is wrong, in words that name the template or value and never repeat a value.</param>
public class EmailCannotBeRendered(string message) : Exception(message);
