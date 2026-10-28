// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Reporting;

/// <summary>
/// One frame of the stack trace captured with an error, top frame first. A part the runtime did not report is
/// <c>NotSet</c>; an error without a stack trace carries no frames at all.
/// </summary>
/// <param name="Method">The method or function.</param>
/// <param name="File">The source file or script URL.</param>
/// <param name="Line">The line number in the file.</param>
public record StackFrame(StackFrameMethod Method, StackFrameFile File, StackFrameLine Line);
