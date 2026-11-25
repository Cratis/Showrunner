// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Cratis.Chronicle;

namespace Cratis.Showrunner.BoxOffice.Contracts.People;

/// <summary>
/// The compliance subject of a person's data, independent of their tenant or organization.
/// </summary>
/// <remarks>
/// Subjects travel unencrypted, so the identity provider and its subject are hashed as a JSON string pair.
/// To erase a person, compute this subject and delete its key in every tenant namespace holding their data.
/// </remarks>
public static class PersonSubjects
{
    const string Prefix = "box-office-person-";

    /// <summary>
    /// Gets the compliance subject from a person's stable identity, never their name or email.
    /// </summary>
    /// <param name="person">The person whose data is protected.</param>
    /// <returns>The <see cref="Subject"/>.</returns>
    public static Subject For(Person person)
    {
        var identity = JsonSerializer.Serialize(new[] { person.IdentityProvider.Value, person.IdentityProviderSubject.Value });
        return new($"{Prefix}{Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identity)))}");
    }
}
