// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using Cratis.Chronicle;

namespace Cratis.Showrunner.Playbill.Viewers;

/// <summary>
/// The identity of a person's own stream of events: what they have seen is recorded under it.
/// </summary>
/// <remarks>
/// It is derived from the person's subject with a one-way hash, so the event source id never carries the subject itself.
/// </remarks>
/// <param name="Value">The underlying value.</param>
public record ViewerId(string Value) : EventSourceId<string>(Value)
{
    const string Prefix = "playbill-viewer-";

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ViewerId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ViewerId(string value) => new(value);

    /// <summary>
    /// Gets the id of a person's stream.
    /// </summary>
    /// <param name="viewer">The person.</param>
    /// <returns>The id.</returns>
    public static ViewerId For(ViewerSubject viewer) =>
        new($"{Prefix}{Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(viewer.Value)))}");

    /// <summary>
    /// Gets the compliance subject of a person: what an erasure request for them is addressed to.
    /// </summary>
    /// <param name="viewer">The person.</param>
    /// <returns>The subject.</returns>
    public static Subject SubjectOf(ViewerSubject viewer) => new(For(viewer).Value);
}
