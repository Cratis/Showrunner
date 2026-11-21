// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.State.for_ConfigMapAlertState;

public class when_the_config_map_does_not_exist : Specification
{
    AlertStateSnapshot _loaded;

    async Task Because()
    {
        var store = Substitute.For<IConfigMapStore>();
        store.Read(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((string?)null);
        _loaded = await new ConfigMapAlertState(store, "cluster-health-watchdog-state").Load(CancellationToken.None);
    }

    [Fact] void should_start_from_empty_state() => _loaded.ShouldEqual(AlertStateSnapshot.Empty);
}
#endif
