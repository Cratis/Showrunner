// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Reporting;

namespace Cratis.Showrunner.Usher.Reporting;

/// <summary>
/// Represents the stand-in for <see cref="IComputeErrorFingerprints"/> until fingerprinting is built
/// (Cratis/Showrunner#4): every error gets <see cref="ErrorFingerprint.NotSet"/>.
/// </summary>
/// <remarks>
/// It records no fingerprint rather than one from a throwaway algorithm, because a fingerprint is stored on the event
/// forever and one computed differently from the real one would group reports wrongly. A desk treats an unset
/// fingerprint as "not grouped". Replace this type and its registration in <c>AddUsher</c> to compute real ones.
/// </remarks>
public class NotYetComputedErrorFingerprints : IComputeErrorFingerprints
{
    /// <inheritdoc/>
    public ErrorFingerprint Compute(
        ApplicationVersion applicationVersion,
        SliceLocation location,
        ErrorType errorType,
        ErrorMessage errorMessage,
        IEnumerable<StackFrame> stackFrames) => ErrorFingerprint.NotSet;
}
