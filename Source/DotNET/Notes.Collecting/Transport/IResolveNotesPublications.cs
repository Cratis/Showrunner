// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Collecting.Transport;

/// <summary>
/// Defines how the host that collects notes authenticates a publication. The host stores the secrets however it likes -
/// a hash of each is the safe way - and the collecting side never sees how.
/// </summary>
/// <remarks>
/// Compare the presented secret with the stored one in constant time, for example
/// <c>CryptographicOperations.FixedTimeEquals</c> over the hashes of both, and answer the same <see langword="null"/> for an
/// unknown publication as for a wrong secret, so a caller learns nothing from the difference.
/// </remarks>
public interface IResolveNotesPublications
{
    /// <summary>
    /// Resolves a publication from its id and the secret presented for it.
    /// </summary>
    /// <param name="publicationId">The publication named in the route.</param>
    /// <param name="bearerToken">The secret the caller presented as a bearer token.</param>
    /// <returns>The publication, or <see langword="null"/> when it is unknown or the secret is wrong.</returns>
    Task<NotesPublication?> Resolve(PublicationId publicationId, string bearerToken);
}
