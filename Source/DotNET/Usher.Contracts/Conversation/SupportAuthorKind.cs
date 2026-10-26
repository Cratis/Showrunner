// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Conversation;

/// <summary>
/// What kind of author wrote a support entry the reporter sees.
/// </summary>
public enum SupportAuthorKind
{
    /// <summary>
    /// A person at the support desk.
    /// </summary>
    Person = 0,

    /// <summary>
    /// A support agent acting for the desk.
    /// </summary>
    Agent = 1
}
