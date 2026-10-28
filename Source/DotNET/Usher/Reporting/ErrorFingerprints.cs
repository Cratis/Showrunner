// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using Cratis.Showrunner.Usher.Contracts.Reporting;

namespace Cratis.Showrunner.Usher.Reporting;

/// <summary>
/// Represents the default <see cref="IComputeErrorFingerprints"/>: a SHA-256 hash of what identifies an error.
/// </summary>
/// <remarks>
/// <para>
/// The hash is taken over the UTF-8 bytes of these lines, joined with <c>\n</c>, and written as 64 lowercase
/// hexadecimal characters:
/// </para>
/// <code>
/// usher-error-fingerprint/v1
/// version=&lt;application version&gt;
/// module=&lt;module&gt;
/// feature=&lt;feature&gt;
/// slice=&lt;slice&gt;
/// type=&lt;error type&gt;
/// message=&lt;message normalized by ErrorMessageNormalization&gt;
/// frame=&lt;method&gt;|&lt;file name&gt;|&lt;line&gt;    (or frame=none)
/// </code>
/// <para>
/// The frame is the top frame, the one closest to where the error was raised. Only the file name is used, without its
/// folder or URL, so the same build served from different hosts or built in different folders gives the same
/// fingerprint. Without stack frames, or when the top frame carries nothing, the line reads <c>frame=none</c>.
/// </para>
/// <para>
/// A fingerprint is stored on the event forever, so this algorithm never changes in place: a different one gets a
/// new first line, and fingerprints from the two never match.
/// </para>
/// </remarks>
public class ErrorFingerprints : IComputeErrorFingerprints
{
    /// <summary>
    /// The first line of what is hashed, naming the algorithm.
    /// </summary>
    public const string Algorithm = "usher-error-fingerprint/v1";

    /// <inheritdoc/>
    public ErrorFingerprint Compute(
        ApplicationVersion applicationVersion,
        SliceLocation location,
        ErrorType errorType,
        ErrorMessage errorMessage,
        IEnumerable<StackFrame> stackFrames)
    {
        var input = string.Join(
            '\n',
            Algorithm,
            $"version={applicationVersion.Value}",
            $"module={location.Module.Value}",
            $"feature={location.Feature.Value}",
            $"slice={location.Slice.Value}",
            $"type={errorType.Value}",
            $"message={ErrorMessageNormalization.Normalize(errorMessage)}",
            $"frame={TopFrameOf(stackFrames)}");

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(hash);
    }

    static string TopFrameOf(IEnumerable<StackFrame> stackFrames)
    {
        var top = stackFrames.FirstOrDefault();
        if (top is null || (string.IsNullOrWhiteSpace(top.Method) && string.IsNullOrWhiteSpace(top.File) && top.Line == StackFrameLine.NotSet))
        {
            return "none";
        }

        return $"{top.Method.Value.Trim()}|{FileNameOf(top.File)}|{top.Line.Value}";
    }

    static string FileNameOf(StackFrameFile file)
    {
        var path = file.Value ?? string.Empty;
        var end = path.IndexOfAny(['?', '#']);
        if (end >= 0)
        {
            path = path[..end];
        }

        var start = path.LastIndexOfAny(['/', '\\']);
        return (start >= 0 ? path[(start + 1)..] : path).Trim();
    }
}
