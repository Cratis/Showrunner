// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Probe;

/// <summary>
/// One request of the synthetic probe and the status it must answer with.
/// </summary>
/// <param name="Id">The id of the check, unique within its target.</param>
/// <param name="Method">The HTTP method.</param>
/// <param name="Path">The path, beginning with a slash.</param>
/// <param name="Accept">The Accept header.</param>
/// <param name="ExpectStatus">The HTTP status the endpoint must answer with. An anonymous 401 is how an endpoint behind an authenticating proxy proves the proxy answers.</param>
/// <param name="Body">The JSON body, or <see langword="null"/> for none.</param>
/// <param name="TimeoutMs">How long to wait, or <see langword="null"/> for the default of ten seconds.</param>
public sealed record ProbeEndpoint(string Id, string Method, string Path, string Accept, int ExpectStatus, string? Body = null, int? TimeoutMs = null);
