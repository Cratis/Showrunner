// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Notebook;

/// <summary>
/// The limits that keep a note a reasonable size.
/// </summary>
public static class NoteLimits
{
    /// <summary>
    /// The longest text a note can have, in characters.
    /// </summary>
    public const int TextLength = 4000;

    /// <summary>
    /// The most slices a note can name, as the subject or as active on the screen.
    /// </summary>
    public const int Slices = 50;

    /// <summary>
    /// The most model references a note can carry.
    /// </summary>
    public const int ModelReferences = 50;

    /// <summary>
    /// The most ancestors a picked component can have.
    /// </summary>
    public const int Ancestors = 50;

    /// <summary>
    /// The longest value of a presentation property, in characters.
    /// </summary>
    public const int PropertyValueLength = 1000;
}
