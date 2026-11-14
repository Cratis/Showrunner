// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Cue.Viewers;

/// <summary>
/// Defines the person signed in to the application, as Cue knows them. An application whose people are not told apart by
/// the <c>sub</c> claim registers its own implementation.
/// </summary>
public interface ISignedInViewer
{
    /// <summary>
    /// Gets a value indicating whether a person is signed in: the principal is authenticated and has a subject.
    /// </summary>
    bool IsSignedIn { get; }

    /// <summary>
    /// Gets the person's subject, <see cref="ViewerSubject.NotSet"/> when no one is signed in.
    /// </summary>
    ViewerSubject Subject { get; }
}
