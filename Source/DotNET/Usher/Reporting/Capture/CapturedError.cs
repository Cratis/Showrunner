// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Reporting;

namespace Cratis.Showrunner.Usher.Reporting.Capture;

/// <summary>
/// Represents an error as the backend saw it.
/// </summary>
/// <param name="Location">Where in the application the error happened.</param>
/// <param name="ErrorType">The type of the error.</param>
/// <param name="ErrorMessage">The message of the error.</param>
/// <param name="StackFrames">The stack trace of the error, top frame first; empty when there is none.</param>
/// <param name="ApplicationVersion">The version of the application the error happened in.</param>
public record CapturedError(
    SliceLocation Location,
    ErrorType ErrorType,
    ErrorMessage ErrorMessage,
    IEnumerable<StackFrame> StackFrames,
    ApplicationVersion ApplicationVersion);
