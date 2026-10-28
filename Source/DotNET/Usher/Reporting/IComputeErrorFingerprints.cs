// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Reporting;

namespace Cratis.Showrunner.Usher.Reporting;

/// <summary>
/// Defines a system that computes the fingerprint of an error, so the desk can group reports of the same error.
/// </summary>
public interface IComputeErrorFingerprints
{
    /// <summary>
    /// Computes the fingerprint of an error.
    /// </summary>
    /// <param name="applicationVersion">The version of the application the error happened in.</param>
    /// <param name="location">Where in the application the error happened.</param>
    /// <param name="errorType">The type of the error.</param>
    /// <param name="errorMessage">The message of the error.</param>
    /// <param name="stackFrames">The stack trace of the error, top frame first; may be empty.</param>
    /// <returns>The <see cref="ErrorFingerprint"/>.</returns>
    ErrorFingerprint Compute(
        ApplicationVersion applicationVersion,
        SliceLocation location,
        ErrorType errorType,
        ErrorMessage errorMessage,
        IEnumerable<StackFrame> stackFrames);
}
