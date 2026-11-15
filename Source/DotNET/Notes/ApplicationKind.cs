// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes;

/// <summary>
/// What kind of running application a note is given in.
/// </summary>
public enum ApplicationKind
{
    /// <summary>
    /// The published application, the one its users use. Notes sends nothing extra, so this is how every note before the kind
    /// could be named was given.
    /// </summary>
    Published = 0,

    /// <summary>
    /// A staged session: a Play, a Preview or a sandbox the builder started to try the application before it is published.
    /// Every note given in it is published together with <c>NoteGivenOnStagedApplication</c>.
    /// </summary>
    Staged = 1
}
