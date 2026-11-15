// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Reporting;
using Cratis.Showrunner.Understudy.Contracts.Backups;

namespace Cratis.Showrunner.StageManager.Understudies.for_KubernetesEventSink;

public class when_publishing_a_fact : Specification
{
    static readonly DateTimeOffset _now = new(2026, 3, 15, 12, 30, 0, TimeSpan.Zero);

    FakeUnderstudyCluster _cluster;
    Understudy _understudy;
    UnderstudyFact _failure;
    UnderstudyFact _success;

    Corev1Event Event(UnderstudyFact fact) => _cluster.Created.OfType<Corev1Event>().Single(item => item.Metadata.Name == KubernetesEventSink.NameOf(_understudy.Metadata.Name, fact.Id));

    void Establish()
    {
        _cluster = new();
        _understudy = UnderstudySamples.Understudy(UnderstudySamples.Spec());
        _failure = new("BackupFailed:job-1", "BackupFailed", new BackupFailed("ledger", "job-1", _now, "backup", "The backup job failed (BackoffLimitExceeded)."), true);
        _success = new("BackupTaken:ledger/Studio/acme/20260315T120000Z", "BackupTaken", new BackupTaken("ledger", "ledger/Studio/acme/20260315T120000Z", "Studio", "acme", _now, ["Events"], 10), false);
    }

    async Task Because()
    {
        var sink = new KubernetesEventSink(_cluster, new FixedTimeProvider(_now));
        await sink.PublishAsync(_understudy, _failure, CancellationToken.None);
        await sink.PublishAsync(_understudy, _failure, CancellationToken.None);
        await sink.PublishAsync(_understudy, _success, CancellationToken.None);
    }

    [Fact] void should_create_one_event_per_fact() => _cluster.Created.OfType<Corev1Event>().Count().ShouldEqual(2);
    [Fact] void should_not_create_a_fact_twice() => _cluster.Created.OfType<Corev1Event>().Count(item => item.Reason == "BackupFailed").ShouldEqual(1);
    [Fact] void should_use_the_event_type_as_the_reason() => Event(_failure).Reason.ShouldEqual("BackupFailed");
    [Fact] void should_show_a_failure_as_a_warning() => Event(_failure).Type.ShouldEqual("Warning");
    [Fact] void should_show_a_success_as_normal() => Event(_success).Type.ShouldEqual("Normal");
    [Fact] void should_carry_the_event_as_json_in_the_message() => Event(_failure).Message.ShouldContain("\"operationId\":\"job-1\"");
    [Fact] void should_involve_the_understudy() => Event(_failure).InvolvedObject.Kind.ShouldEqual("Understudy");
    [Fact] void should_involve_it_by_uid() => Event(_failure).InvolvedObject.Uid.ShouldEqual(_understudy.Metadata.Uid);
    [Fact] void should_live_in_the_namespace_of_the_understudy() => Event(_failure).Metadata.NamespaceProperty.ShouldEqual("apps");
    [Fact] void should_name_the_event_from_the_fact_id() => KubernetesEventSink.NameOf("a", "x").ShouldEqual(KubernetesEventSink.NameOf("a", "x"));
    [Fact] void should_give_each_fact_its_own_name() => KubernetesEventSink.NameOf("a", "x").ShouldNotEqual(KubernetesEventSink.NameOf("a", "y"));
}
#endif
