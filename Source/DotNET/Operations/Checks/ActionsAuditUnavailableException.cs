// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// Thrown inside the GitHub Actions audit when every eligible repository refused it, so nothing could be audited.
/// </summary>
public sealed class ActionsAuditUnavailableException() : Exception("no repository is accessible");
