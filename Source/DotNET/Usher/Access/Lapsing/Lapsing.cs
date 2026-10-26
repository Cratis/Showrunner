// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Access;

namespace Cratis.Showrunner.Usher.Access.Lapsing;

/// <summary>
/// A grant of access ended because its time passed without the reporter revoking it. The lapsing automation appends
/// it when <see cref="ValidUntil"/> passes (Cratis/Showrunner#3); it is published as <c>AccessExpired</c>.
/// </summary>
/// <param name="ValidUntil">The end of the grant that lapsed, so a lapse of an earlier grant never ends a later one.</param>
[EventType]
public record AccessGrantLapsed(ValidUntil ValidUntil);
