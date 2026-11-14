// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using Cratis.Chronicle;

namespace Cratis.Showrunner.Cue.Viewers;

/// <summary>
/// The event source a person's guided-tour history is recorded under. It is derived from the person's
/// <see cref="ViewerSubject"/> so the subject itself never becomes an event source id, which is stored in the clear.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ViewerId(string Value) : EventSourceId<string>(Value)
{
    const string Prefix = "cue-viewer-";

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ViewerId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ViewerId(string value) => new(value);

    /// <summary>
    /// Gets the id of the person a subject names.
    /// </summary>
    /// <param name="viewer">The person.</param>
    /// <returns>The <see cref="ViewerId"/> their tours are recorded under.</returns>
    public static ViewerId For(ViewerSubject viewer) =>
        new($"{Prefix}{Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(viewer.Value)))}");

    /// <summary>
    /// Gets the compliance subject of the person a subject names, so an erasure request finds what Cue recorded about them.
    /// </summary>
    /// <param name="viewer">The person.</param>
    /// <returns>The compliance <see cref="Subject"/>.</returns>
    public static Subject SubjectOf(ViewerSubject viewer) => new(For(viewer).Value);
}
