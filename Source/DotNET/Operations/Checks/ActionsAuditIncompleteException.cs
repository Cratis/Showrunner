// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// Thrown inside the GitHub Actions audit when its request or time budget runs out before it has seen everything.
/// </summary>
public sealed class ActionsAuditIncompleteException() : Exception("the audit budget is exhausted");
