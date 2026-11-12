// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.GrantAudience.when_granting_an_audience;

/// <summary>
/// An audience name must be one a model can declare: a letter first, then letters, digits and underscores.
/// </summary>
public class an_audience_name_that_is_not_valid : a_guest_list_command<GrantAudience>
{
    CommandResult _withSpace;
    CommandResult _startingWithDigit;
    CommandResult _tooLong;

    async Task Because()
    {
        _result = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.Tenant, Guests.Acme, UserId.NotSet, "Plus Plan"));
        _withSpace = _result;
        _startingWithDigit = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.Tenant, Guests.Acme, UserId.NotSet, "2Plus"));
        _tooLong = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.Tenant, Guests.Acme, UserId.NotSet, new string('a', AudienceName.MaxLength + 1)));
    }

    [Fact] void should_refuse_a_name_with_a_space() => _withSpace.ShouldNotBeSuccessful();

    [Fact] void should_refuse_a_name_starting_with_a_digit() => _startingWithDigit.ShouldNotBeSuccessful();

    [Fact] void should_refuse_a_name_that_is_too_long() => _tooLong.ShouldNotBeSuccessful();

    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
