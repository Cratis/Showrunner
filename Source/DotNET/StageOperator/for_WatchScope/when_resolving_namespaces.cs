// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.Hosting;

namespace Cratis.Showrunner.StageOperator.for_WatchScope;

public class when_resolving_namespaces : Specification
{
    [Fact] void should_watch_every_namespace_when_none_is_configured() => Assert.Equal<string?>([null], WatchScope.Namespaces(new StageOperatorOptions()));
    [Fact] void should_watch_only_the_configured_namespaces() => Assert.Equal<string?>(["a", "b"], WatchScope.Namespaces(new StageOperatorOptions { WatchNamespaces = ["a", " b ", "a", ""] }));
}
#endif
