// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Reflection;
using Cratis.Showrunner.Usher.Identity;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation;

public class when_inspecting_the_shape_of_the_read_model : Specification
{
    static readonly string[] _parameterNames =
    [
        "Id",
        "Title",
        "State",
        "Location",
        "ReportedAt",
        "IdentityShared",
        "OpenQuestion",
        "Entries",
        "Access",
        "KnownIssueTitle",
        "WorkItem",
        "FixedInVersion",
        "ReportedBy"
    ];

    ParameterInfo[] _parameters;
    Type[] _reducedEvents;
    MethodInfo[] _publicStatics;

    void Because()
    {
        var type = typeof(IssueConversation);
        _parameters = type.GetConstructors().Single().GetParameters();
        _publicStatics = [.. type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(method => !method.IsSpecialName)];
        _reducedEvents = [.. _publicStatics.Where(method => method.Name == "Reduce").Select(method => method.GetParameters()[0].ParameterType)];
    }

    [Fact] void should_keep_the_thirteen_constructor_parameters() => _parameters.Length.ShouldEqual(13);
    [Fact] void should_keep_the_names_of_the_constructor_parameters_in_order() => _parameters.Select(parameter => parameter.Name).ShouldEqual<string?>(_parameterNames);
    [Fact] void should_keep_the_issue_id_first() => _parameters[0].ParameterType.ShouldEqual(typeof(IssueId));
    [Fact] void should_keep_the_reporter_last() => _parameters[12].ParameterType.ShouldEqual(typeof(UserIdentity));
    [Fact] void should_keep_the_access_parameter_type() => _parameters[8].ParameterType.ShouldEqual(typeof(IssueAccess));
    [Fact] void should_reduce_twenty_five_events() => _reducedEvents.Length.ShouldEqual(25);
    [Fact] void should_reduce_each_event_once() => _reducedEvents.Distinct().Count().ShouldEqual(25);
    [Fact] void should_keep_the_issue_query() => _publicStatics.Count(method => method.Name == "ForIssue").ShouldEqual(1);
}
#endif
