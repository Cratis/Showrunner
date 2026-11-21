// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// Thrown when a GitHub answer is larger than the audit is willing to read; the audit is then incomplete.
/// </summary>
public sealed class GitHubResponseTooLargeException() : Exception("a GitHub answer exceeded the size limit");
