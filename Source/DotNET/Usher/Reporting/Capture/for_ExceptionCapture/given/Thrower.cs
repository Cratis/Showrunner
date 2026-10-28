// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Cratis.Showrunner.Usher.Reporting.Capture.for_ExceptionCapture.given;

/// <summary>
/// Throws exceptions from a known place.
/// </summary>
public static class Thrower
{
    /// <summary>
    /// The message of the exceptions thrown.
    /// </summary>
    public const string Message = "Invoice 42 is already sent";

    /// <summary>
    /// Throws and catches an exception, so it carries a stack trace.
    /// </summary>
    /// <returns>The thrown exception.</returns>
    /// <exception cref="UnreachableException">Never: the exception is always caught.</exception>
    public static Exception Thrown()
    {
        try
        {
            Throw();
        }
        catch (InvalidOperationException exception)
        {
            return exception;
        }

        throw new UnreachableException();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static void Throw() => throw new InvalidOperationException(Message);
}
#endif
