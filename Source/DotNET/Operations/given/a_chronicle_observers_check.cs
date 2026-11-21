// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Checks;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// The Chronicle observers check against a Chronicle with two event store namespaces and nothing failing.
/// </summary>
public abstract class a_chronicle_observers_check : a_check
{
    protected FakeChronicleReader _chronicle;
    IWatchdogCheck _check;

    protected override IWatchdogCheck Check => _check ??= new ChronicleObserversCheck(_chronicle, new ChronicleObserversOptions());

    /// <summary>
    /// Starts the next watchdog run, which reads Chronicle afresh.
    /// </summary>
    protected void NextRun() => _check = null;

    protected ChronicleFailedPartition Partition(string observer, int minutes, bool quarantined = false, bool resolved = false) =>
        FakeChronicleReader.Partition(observer, TimeSpan.FromMinutes(minutes), _time.Now, quarantined, resolved);

    void Establish()
    {
        _chronicle = new FakeChronicleReader();
        _chronicle.Stores["studio"] = new Dictionary<string, List<ChronicleFailedPartition>>
        {
            ["acme"] = [],
            ["globex"] = []
        };
    }
}
#endif
