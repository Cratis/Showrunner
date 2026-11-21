// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.State;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_a_check_keeps_state_of_its_own : a_watchdog_run
{
    sealed class Counting : IPersistentCheck
    {
        public string? Restored { get; private set; }

        public int Runs { get; private set; }

        public string Name => "counting";

        public void Restore(string? json) => Restored = json;

        public string? Persist() => $"{{\"runs\":{Runs}}}";

        public Task<IReadOnlyCollection<Finding>> Collect(CheckContext context, CancellationToken cancellationToken)
        {
            Runs++;
            return Task.FromResult<IReadOnlyCollection<Finding>>([]);
        }
    }

    Counting _check;

    void Establish()
    {
        _check = new Counting();
        _checks.Add(_check);
        _state.Current = AlertStateSnapshot.Empty with { ActionsAudit = "{\"runs\":41}" };
    }

    async Task Because() => await CreateRun().Run(CancellationToken.None);

    [Fact] void should_hand_the_saved_state_to_the_check_before_it_runs() => _check.Restored.ShouldEqual("{\"runs\":41}");
    [Fact] void should_save_the_state_the_check_asks_for() => _state.Current.ActionsAudit.ShouldEqual("{\"runs\":1}");
}
#endif
