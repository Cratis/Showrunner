// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Collecting.Receiving;

namespace Cratis.Showrunner.Notes.Collecting.Transport;

/// <summary>
/// What the collector knows about a publication once it has authenticated it.
/// </summary>
/// <param name="TenantId">The tenant of the collecting store the publication's notes are recorded for: the name of its namespace. The notes' own <c>TenantId</c> must equal it.</param>
/// <param name="AppStore">The name the notes' source is recorded under, as <c>AppStore</c> on every received fact.</param>
public record NotesPublication(TenantId TenantId, AppStoreName AppStore);
