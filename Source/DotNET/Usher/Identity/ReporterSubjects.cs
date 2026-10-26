// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;

namespace Cratis.Showrunner.Usher.Identity;

/// <summary>
/// The compliance subject of everything a reporter writes: the person, not the issue. Every fact the reporter records
/// on any of their issues is encrypted under the same subject's key, so erasing the subject erases what they wrote on
/// all of their issues at once.
/// </summary>
/// <remarks>
/// The subject travels unencrypted with every event, also to the desk, and the desk is not meant to learn who reported
/// an issue until the reporter shares who they are. The subject is therefore a one-way hash of the identity provider
/// and the subject, not the subject itself. To erase a reporter, compute it with <see cref="For"/> from their identity
/// provider and subject and delete its key in every tenant namespace they reported in.
/// </remarks>
public static class ReporterSubjects
{
    const string Prefix = "usher-reporter-";

    /// <summary>
    /// Gets the compliance subject of a reporter.
    /// </summary>
    /// <param name="reporter">Who the reporter is at their identity provider.</param>
    /// <returns>The <see cref="Subject"/>.</returns>
    public static Subject For(UserIdentity reporter)
    {
        var identity = $"{reporter.IdentityProvider.Value}\n{reporter.Subject.Value}";
        return new($"{Prefix}{Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identity)))}");
    }

    /// <summary>
    /// Wraps a fact the reporter records on an issue so it is appended with the reporter as its compliance subject.
    /// </summary>
    /// <param name="issueId">The issue.</param>
    /// <param name="reporter">Who the reporter is at their identity provider.</param>
    /// <param name="fact">The fact.</param>
    /// <returns>The <see cref="EventForEventSourceId"/>.</returns>
    public static EventForEventSourceId Fact(IssueId issueId, UserIdentity reporter, object fact) =>
        new(issueId, fact) { Subject = For(reporter) };
}
