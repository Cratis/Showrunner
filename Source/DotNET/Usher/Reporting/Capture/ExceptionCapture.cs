// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Reflection;
using Cratis.Showrunner.Usher.Contracts.Reporting;
using UsherStackFrame = Cratis.Showrunner.Usher.Contracts.Reporting.StackFrame;

namespace Cratis.Showrunner.Usher.Reporting.Capture;

/// <summary>
/// Turns a .NET exception into a <see cref="CapturedError"/>.
/// </summary>
public static class ExceptionCapture
{
    /// <summary>
    /// Captures an exception.
    /// </summary>
    /// <remarks>
    /// A <see cref="TargetInvocationException"/>, and an <see cref="AggregateException"/> with a single inner exception,
    /// are unwrapped first, so the error is the one the code raised and not the wrapper reflection or a task added.
    /// </remarks>
    /// <param name="exception">The <see cref="Exception"/> to capture.</param>
    /// <param name="location">Where in the application the exception happened.</param>
    /// <param name="applicationVersion">The version of the application.</param>
    /// <param name="maximumStackFrames">How many stack frames, from the top, to keep.</param>
    /// <returns>The <see cref="CapturedError"/>.</returns>
    public static CapturedError Capture(Exception exception, SliceLocation location, ApplicationVersion applicationVersion, int maximumStackFrames)
    {
        var error = Unwrap(exception);
        var type = error.GetType();
        return new(
            location,
            type.FullName ?? type.Name,
            error.Message,
            StackFramesOf(error, maximumStackFrames),
            applicationVersion);
    }

    /// <summary>
    /// Gets the stack frames of an exception, top frame first, from the runtime's stack trace rather than its text, so
    /// they read the same whatever the culture of the process.
    /// </summary>
    /// <param name="exception">The <see cref="Exception"/>.</param>
    /// <param name="maximumStackFrames">How many stack frames, from the top, to keep.</param>
    /// <returns>The stack frames; empty when the exception was never thrown or has no frames with a known method.</returns>
    public static IEnumerable<UsherStackFrame> StackFramesOf(Exception exception, int maximumStackFrames)
    {
        if (maximumStackFrames <= 0)
        {
            return [];
        }

        return new StackTrace(exception, fNeedFileInfo: true)
            .GetFrames()
            .Select(ToStackFrame)
            .OfType<UsherStackFrame>()
            .Take(maximumStackFrames)
            .ToArray();
    }

    static UsherStackFrame? ToStackFrame(System.Diagnostics.StackFrame frame)
    {
        if (DiagnosticMethodInfo.Create(frame) is not { } method)
        {
            return null;
        }

        var name = string.IsNullOrEmpty(method.DeclaringTypeName) ? method.Name : $"{method.DeclaringTypeName}.{method.Name}";
        var file = frame.GetFileName();
        var line = frame.GetFileLineNumber();
        return new(
            name,
            string.IsNullOrEmpty(file) ? StackFrameFile.NotSet : file,
            line > 0 ? line : StackFrameLine.NotSet);
    }

    static Exception Unwrap(Exception exception) => exception switch
    {
        TargetInvocationException { InnerException: { } inner } => Unwrap(inner),
        AggregateException aggregate when aggregate.InnerExceptions.Count == 1 => Unwrap(aggregate.InnerExceptions[0]),
        _ => exception
    };
}
