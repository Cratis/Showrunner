// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Reflection;

namespace Cratis.Showrunner.Notes.Collecting.Notebook.for_ReceivedNote;

public class the_shape : Specification
{
    ParameterInfo[] _parameters;
    int _reducers;

    void Because()
    {
        _parameters = typeof(ReceivedNote).GetConstructors().Single(_ => _.GetParameters().Length == 5).GetParameters();
        _reducers = typeof(ReceivedNote).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Count(_ => _.Name == "Reduce");
    }

    [Fact] void should_have_the_five_constructor_parameters_in_order() =>
        string.Join(',', _parameters.Select(_ => $"{_.ParameterType.Name} {_.Name}")).ShouldEqual("NoteId Id,Boolean IsWithdrawn,Boolean IsAcknowledged,AddressedInVersion AddressedInVersion,Boolean IsDeclined");

    [Fact] void should_have_nine_reducers() => _reducers.ShouldEqual(9);
}
#endif
