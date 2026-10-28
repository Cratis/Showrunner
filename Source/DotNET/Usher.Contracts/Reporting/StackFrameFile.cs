// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Reporting;

/// <summary>
/// The source file, or for frontend errors the script URL, a <see cref="StackFrame"/> points at.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record StackFrameFile(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="StackFrameFile"/>, used when the runtime did not report one (for example
    /// without debug symbols).
    /// </summary>
    public static readonly StackFrameFile NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="StackFrameFile"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator StackFrameFile(string value) => new(value);
}
