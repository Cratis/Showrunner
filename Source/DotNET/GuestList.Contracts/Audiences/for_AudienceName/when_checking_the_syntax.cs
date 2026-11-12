// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.GuestList.Contracts.Audiences.for_AudienceName;

/// <summary>
/// An audience name starts with a letter and holds letters, digits and underscores, up to 64 characters.
/// </summary>
public class when_checking_the_syntax : Specification
{
    bool _plus;
    bool _withDigitsAndUnderscore;
    bool _empty;
    bool _startingWithDigit;
    bool _withSpace;
    bool _tooLong;
    bool _longest;
    bool _missing;

    void Because()
    {
        _plus = AudienceName.IsValidName("Plus");
        _withDigitsAndUnderscore = AudienceName.IsValidName("Early_Access2");
        _empty = AudienceName.IsValidName(string.Empty);
        _startingWithDigit = AudienceName.IsValidName("2Plus");
        _withSpace = AudienceName.IsValidName("Plus Plan");
        _tooLong = AudienceName.IsValidName(new string('a', AudienceName.MaxLength + 1));
        _longest = AudienceName.IsValidName(new string('a', AudienceName.MaxLength));
        _missing = AudienceName.IsValidName(null);
    }

    [Fact] void should_accept_a_plain_name() => Assert.True(_plus);
    [Fact] void should_accept_digits_and_underscores() => Assert.True(_withDigitsAndUnderscore);
    [Fact] void should_accept_the_longest_name() => Assert.True(_longest);
    [Fact] void should_refuse_an_empty_name() => Assert.False(_empty);
    [Fact] void should_refuse_a_name_starting_with_a_digit() => Assert.False(_startingWithDigit);
    [Fact] void should_refuse_a_name_with_a_space() => Assert.False(_withSpace);
    [Fact] void should_refuse_a_name_that_is_too_long() => Assert.False(_tooLong);
    [Fact] void should_refuse_no_name() => Assert.False(_missing);
}
#endif
