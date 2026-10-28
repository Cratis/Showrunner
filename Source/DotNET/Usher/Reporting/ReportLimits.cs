// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Reporting;

/// <summary>
/// The most a reporter can send in one command. Everything a reporter sends is stored forever, published to the desk
/// and shown to support, so every text and every list has a bound; a command over one is refused with a validation
/// error. The limits are generous for what a person writes or a browser reports, and the generated proxies check them
/// before a command is sent.
/// </summary>
public static class ReportLimits
{
    /// <summary>
    /// The longest summary a reporter can give; it is the issue's title.
    /// </summary>
    public const int SummaryLength = 120;

    /// <summary>
    /// The longest text a reporter can write in one go: a description, details or an answer.
    /// </summary>
    public const int TextLength = 4000;

    /// <summary>
    /// The longest reason a reporter can give for reopening an issue.
    /// </summary>
    public const int ReopenReasonLength = 1000;

    /// <summary>
    /// The longest error message a report can carry.
    /// </summary>
    public const int ErrorMessageLength = 2000;

    /// <summary>
    /// The longest error type a report can carry.
    /// </summary>
    public const int ErrorTypeLength = 500;

    /// <summary>
    /// The most stack frames a report can carry; the same as the default of
    /// <see cref="Capture.ErrorCaptureOptions.MaximumStackFrames"/>.
    /// </summary>
    public const int StackFrames = 50;

    /// <summary>
    /// The longest method or file of a stack frame.
    /// </summary>
    public const int StackFrameTextLength = 1000;

    /// <summary>
    /// The most correlation ids a report can carry.
    /// </summary>
    public const int CorrelationIds = 20;
}
