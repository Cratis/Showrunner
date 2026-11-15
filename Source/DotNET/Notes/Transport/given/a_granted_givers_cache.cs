// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Access;
using Cratis.Showrunner.Notes.Contracts.Access;
using Cratis.Showrunner.Notes.Contracts.Transport;
using Cratis.Showrunner.Notes.given;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Transport.given;

/// <summary>
/// A <see cref="GrantedGiversCache"/> over a collector a specification controls, for an application that lets only granted
/// users give feedback, refreshes every 30 seconds and keeps a list for at most five minutes without confirming it.
/// </summary>
public class a_granted_givers_cache : Specification
{
    protected static readonly TimeSpan Staleness = TimeSpan.FromMinutes(5);

    protected RecordingCollector _collector;
    protected SteppedTime _time;
    protected GrantedGiversCache _cache;

    /// <summary>
    /// Gets the id of the user the specifications act as.
    /// </summary>
    protected static FeedbackGiverId TheGiver => FeedbackGiverId.For(Givers.Identity);

    /// <summary>
    /// Gets the id of someone the desk granted.
    /// </summary>
    protected static FeedbackGiverId SomeoneElse => FeedbackGiverId.For(new(Givers.IdentityProvider, new(Givers.SomeoneElse.Value)));

    void Establish()
    {
        _collector = new();
        _time = new();
        _cache = new(
            _collector,
            Options.Create(new NotesOptions
            {
                Transport = NotesTransport.Http,
                AccessMode = FeedbackAccessMode.GrantedUsers,
                AccessRefreshInterval = TimeSpan.FromSeconds(30),
                AccessMaxStaleness = Staleness
            }),
            _time);
    }

    /// <summary>
    /// Sets what the collector serves.
    /// </summary>
    /// <param name="version">The version of the list.</param>
    /// <param name="givers">The users it names.</param>
    protected void Serve(ulong version, params FeedbackGiverId[] givers) =>
        _collector.Grants = new FeedbackAccessGrants(version, [.. givers.Select(_ => _.Value)]);
}
#endif
