// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using Cratis.Chronicle;

namespace Cratis.Showrunner.Marquee.People;

/// <summary>
/// The compliance subject of what a person has seen: the person, not the announcement. Erasing the subject erases what
/// they have acknowledged on every announcement at once.
/// </summary>
/// <remarks>
/// The subject travels unencrypted with every event, so it is a one-way hash of the person's subject, not the subject
/// itself. To erase a person, compute it with <see cref="For"/> from their subject and delete its key in every tenant
/// namespace they signed in to.
/// </remarks>
public static class PersonSubjects
{
    const string Prefix = "marquee-person-";

    /// <summary>
    /// Gets the compliance subject of a person.
    /// </summary>
    /// <param name="person">Who the person is at their identity provider.</param>
    /// <returns>The <see cref="Subject"/>.</returns>
    public static Subject For(UserSubject person) =>
        new($"{Prefix}{Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(person.Value)))}");
}
