// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Checks;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// A runner pool of the scale sets <c>big</c> and <c>small</c> with a ninety minute job limit and a thirty minute pending limit.
/// </summary>
public abstract class a_runner_pool : a_check
{
    protected FakeRunnerCluster _cluster;
    protected FakeConfigMapStore _store;
    protected RunnerPoolOptions _options = new() { Namespace = "arc-runners", ScaleSets = ["big", "small"] };

    protected override IWatchdogCheck Check => new RunnerPoolCheck(_cluster, _store, _options);

    protected static TimeSpan Minutes(int minutes) => TimeSpan.FromMinutes(minutes);

    protected RunnerInfo Runner(string name, string? job = null, string scaleSet = "big", int ageMinutes = 1, string phase = "Running") =>
        new(name, scaleSet, _time.Now - Minutes(ageMinutes), phase, job is not null, job is null ? null : "Cratis/Studio", job);

    protected void FirstSeen(string runner, int minutesAgo) =>
        _store.Values[("runner-watchdog-state", "busy")] = Merge(_store.Values.TryGetValue(("runner-watchdog-state", "busy"), out var current) ? current : "{}", runner, _time.Now.ToUnixTimeSeconds() - (minutesAgo * 60L));

    protected double Saved(string runner) =>
        System.Text.Json.Nodes.JsonNode.Parse(_store.Values[("runner-watchdog-state", "busy")])![runner]!.GetValue<double>();

    protected string[] SavedRunners() =>
        [.. ((System.Text.Json.Nodes.JsonObject)System.Text.Json.Nodes.JsonNode.Parse(_store.Values[("runner-watchdog-state", "busy")])!).Select(entry => entry.Key)];

    static string Merge(string json, string runner, long seconds)
    {
        var node = (System.Text.Json.Nodes.JsonObject)System.Text.Json.Nodes.JsonNode.Parse(json)!;
        node[runner] = seconds;
        return node.ToJsonString();
    }

    void Establish()
    {
        _cluster = new FakeRunnerCluster();
        _store = new FakeConfigMapStore();
    }
}
#endif
