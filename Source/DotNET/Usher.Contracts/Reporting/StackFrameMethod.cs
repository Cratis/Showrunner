// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Reporting;

/// <summary>
/// The method or function a <see cref="StackFrame"/> points at, including its declaring type when there is one.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record StackFrameMethod(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="StackFrameMethod"/>, used when the runtime did not report one.
    /// </summary>
    public static readonly StackFrameMethod NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="StackFrameMethod"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator StackFrameMethod(string value) => new(value);
}
