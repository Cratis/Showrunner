// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Suspension;

namespace Cratis.Showrunner.BoxOffice.given;

/// <summary>
/// A command that changes something and has no marker.
/// </summary>
public class AnOrdinaryChange;

/// <summary>
/// A command a read-only tenant may still execute.
/// </summary>
[AllowedWhileSuspended]
public class ReportingAProblem;
#endif
