// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Reporting;

/// <summary>
/// The one-based line number in the <see cref="StackFrameFile"/> a <see cref="StackFrame"/> points at.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record StackFrameLine(int Value) : ConceptAs<int>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="StackFrameLine"/>: line numbers are one-based, so zero means the runtime did
    /// not report one.
    /// </summary>
    public static readonly StackFrameLine NotSet = new(0);

    /// <summary>
    /// Implicitly converts an <see cref="int"/> to a <see cref="StackFrameLine"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator StackFrameLine(int value) => new(value);
}
