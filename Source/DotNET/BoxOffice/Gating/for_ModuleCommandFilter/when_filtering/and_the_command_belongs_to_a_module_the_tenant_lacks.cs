// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Gating.for_ModuleCommandFilter.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Gating.for_ModuleCommandFilter.when_filtering;

public class and_the_command_belongs_to_a_module_the_tenant_lacks : a_module_command_filter
{
    void Establish() => _gate.RefusedModuleOf(typeof(AnOrdinaryChange).Namespace).Returns(new ModuleId("analytics"));

    async Task Because() => await Filter(typeof(AnOrdinaryChange));

    [Fact] void should_refuse_the_command() => _result.ShouldNotBeAuthorized();
    [Fact] void should_say_the_module_is_not_entitled() => _result.AuthorizationFailureReason.ShouldEqual(GatingReasons.ModuleNotEntitled);
}
#endif
