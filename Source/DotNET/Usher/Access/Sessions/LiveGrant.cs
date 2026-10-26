// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.Contracts.Identity;

namespace Cratis.Showrunner.Usher.Access.Sessions;

/// <summary>
/// A grant that lets support act as the reporter right now.
/// </summary>
/// <param name="ValidUntil">When the grant ends.</param>
/// <param name="Reporter">Who support acts as, as the reporter shared it.</param>
public record LiveGrant(ValidUntil ValidUntil, Reporter Reporter);
