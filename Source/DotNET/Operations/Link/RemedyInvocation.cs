// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// One decision to run a remedy.
/// </summary>
/// <param name="Remedy">The remedy as the held configuration defines it.</param>
/// <param name="Secrets">The secrets of the held configuration, for a remedy that authenticates.</param>
/// <param name="Offline">Whether it runs under the offline policy rather than a Direct authorization.</param>
public sealed record RemedyInvocation(RemedyDefinition Remedy, IReadOnlyDictionary<string, string> Secrets, bool Offline);
