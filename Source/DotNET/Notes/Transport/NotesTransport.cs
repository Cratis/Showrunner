// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes;

/// <summary>
/// How the application moves its notes to the receiving side and gets the answers back.
/// </summary>
public enum NotesTransport
{
    /// <summary>
    /// Chronicle's outbox, subscription and inbox. The application and the receiving side share a Chronicle kernel.
    /// </summary>
    SameKernel = 0,

    /// <summary>
    /// An HTTP relay to the collector, for an application that runs its own Chronicle kernel.
    /// </summary>
    Http = 1
}
