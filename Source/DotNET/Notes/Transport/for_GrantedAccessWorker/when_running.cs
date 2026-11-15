// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Notes.Access;
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Transport.given;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Transport.for_GrantedAccessWorker;

/// <summary>
/// The worker asks the collector for the granted users when the application needs them, keeps running when the collector
/// fails, and does nothing when the application does not take its granted users from the collector.
/// </summary>
public class when_running : Specification
{
    const string Secret = "s3cret-value-of-the-publication";

    static NotesOptions Needed => new()
    {
        Transport = NotesTransport.Http,
        AccessMode = FeedbackAccessMode.GrantedUsers,
        CollectorToken = Secret,
        AccessRefreshInterval = TimeSpan.FromMilliseconds(5),
        AccessMaxStaleness = TimeSpan.FromMinutes(1)
    };

    (GrantedAccessWorker Worker, RecordingCollector Collector, GrantedGiversCache Cache, RecordingLogger<GrantedAccessWorker> Logger) Create(NotesOptions configured)
    {
        var collector = new RecordingCollector { Grants = new(3, [Contracts.Access.FeedbackGiverId.For(Givers.Identity).Value]) };
        var options = Options.Create(configured);
        var cache = new GrantedGiversCache(collector, options, TimeProvider.System);
        var logger = new RecordingLogger<GrantedAccessWorker>();
        return (new GrantedAccessWorker(cache, options, TimeProvider.System, logger), collector, cache, logger);
    }

    static async Task WaitFor(Func<bool> condition)
    {
        for (var attempt = 0; attempt < 500 && !condition(); attempt++)
        {
            await Task.Delay(10);
        }
    }

    bool _fetched;
    bool _grantedAfterFailing;
    bool _askedWhenNotNeeded;
    Exception _stopped;
    LogLevel _failureLevel;
    bool _secretLogged;

    async Task Because()
    {
        var (worker, _, cache, _) = Create(Needed);
        await worker.StartAsync(CancellationToken.None);
        await WaitFor(() => cache.HasList);
        _fetched = cache.IsGranted(Contracts.Access.FeedbackGiverId.For(Givers.Identity));
        await worker.StopAsync(CancellationToken.None);

        var (failing, collector, failingCache, logger) = Create(Needed);
        collector.FailWith = new CollectorRequestFailed(HttpStatusCode.ServiceUnavailable);
        await failing.StartAsync(CancellationToken.None);
        await WaitFor(() => logger.Messages.Count > 0);
        _stopped = await Record.ExceptionAsync(() => failing.StopAsync(CancellationToken.None));
        _grantedAfterFailing = failingCache.IsGranted(Contracts.Access.FeedbackGiverId.For(Givers.Identity));
        _failureLevel = logger.Messages[0].Level;
        _secretLogged = logger.Messages.Any(_ => _.Text.Contains(Secret, StringComparison.Ordinal));

        var everyone = Needed;
        everyone.AccessMode = FeedbackAccessMode.Everyone;
        var (idle, idleCollector, _, _) = Create(everyone);
        await idle.StartAsync(CancellationToken.None);
        await Task.Delay(100);
        await idle.StopAsync(CancellationToken.None);
        _askedWhenNotNeeded = idleCollector.AccessAskedWith.Count > 0;
    }

    [Fact] void should_keep_the_list_the_collector_serves() => _fetched.ShouldBeTrue();
    [Fact] void should_log_a_failure_as_a_warning() => _failureLevel.ShouldEqual(LogLevel.Warning);
    [Fact] void should_stop_cleanly_after_a_failure() => _stopped.ShouldBeNull();
    [Fact] void should_not_grant_anyone_when_the_first_fetch_failed() => _grantedAfterFailing.ShouldBeFalse();
    [Fact] void should_not_log_the_secret() => _secretLogged.ShouldBeFalse();
    [Fact] void should_not_ask_the_collector_when_everyone_may_give_feedback() => _askedWhenNotNeeded.ShouldBeFalse();
    [Fact] void should_say_it_is_needed_for_granted_users_over_http() => GrantedAccessWorker.IsNeeded(Needed).ShouldBeTrue();
    [Fact] void should_say_it_is_not_needed_in_the_same_kernel() => GrantedAccessWorker.IsNeeded(new NotesOptions { AccessMode = FeedbackAccessMode.GrantedUsers }).ShouldBeFalse();
}
#endif
