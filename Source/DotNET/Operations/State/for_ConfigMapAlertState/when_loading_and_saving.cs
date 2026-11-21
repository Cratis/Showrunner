// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.State.for_ConfigMapAlertState;

public class when_loading_and_saving : Specification
{
    IConfigMapStore _store;
    ConfigMapAlertState _state;
    AlertStateSnapshot _loaded;

    void Establish()
    {
        _store = Substitute.For<IConfigMapStore>();
        _store.Read("cluster-health-watchdog-state", "state", Arg.Any<CancellationToken>()).Returns("""{"a": 1790000000, "__last_check": 1790000100}""");
        _state = new ConfigMapAlertState(_store, "cluster-health-watchdog-state");
    }

    async Task Because()
    {
        _loaded = await _state.Load(CancellationToken.None);
        await _state.Save(_loaded, CancellationToken.None);
    }

    [Fact] void should_read_the_state_key_of_the_config_map() => _loaded.LastAlerted.ContainsKey("a").ShouldBeTrue();
    [Fact] void should_read_the_last_check() => _loaded.LastCheck!.Value.ToUnixTimeSeconds().ShouldEqual(1790000100);
    [Fact] void should_write_the_same_key_back() => _store.Received(1).Write("cluster-health-watchdog-state", "state", Arg.Is<string>(json => json.Contains("\"a\":1790000000")), Arg.Any<CancellationToken>());
}
#endif
