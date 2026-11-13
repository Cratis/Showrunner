// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;
using Cratis.Showrunner.GuestList.Contracts.Availability;

namespace Cratis.Showrunner.GuestList.Authoring.Availability.for_DeclaredAudiencesOf.when_deciding;

/// <summary>
/// What the product reported for its primary environment beats what is configured; configuration is the fallback for a
/// product that has not reported, and a product that reported only from another environment has not reported for this one.
/// </summary>
public class what_a_product_declares : Specification
{
    static readonly GuestListAuthoringOptions _configured = new()
    {
        DeclaredAudiences = new Dictionary<Product, IEnumerable<AudienceName>> { [Guests.Product] = [Guests.Plus, Guests.Preview] }
    };

    static readonly GuestListAuthoringOptions _notConfigured = new();

    static ProductAvailability Reported(EnvironmentName environment, params AudienceName[] audiences) =>
        new(
            Guests.Product,
            [
                new EnvironmentAvailability(
                    environment,
                    "1.0.0",
                    DateTimeOffset.UnixEpoch,
                    string.Empty,
                    false,
                    [.. audiences.Select(audience => new AudienceDeclaration(audience, string.Empty, string.Empty))],
                    [],
                    [],
                    [],
                    [])
            ],
            []);

    IEnumerable<AudienceName>? _reportedBeatsConfigured;
    IEnumerable<AudienceName>? _nothingReported;
    IEnumerable<AudienceName>? _reportedFromAnotherEnvironment;
    IEnumerable<AudienceName>? _unnamedEnvironment;
    IEnumerable<AudienceName>? _nothingKnown;

    void Because()
    {
        _reportedBeatsConfigured = DeclaredAudiencesOf.Product(Guests.Product, [Reported("Production", Guests.Plus)], _configured);
        _nothingReported = DeclaredAudiencesOf.Product(Guests.Product, [], _configured);
        _reportedFromAnotherEnvironment = DeclaredAudiencesOf.Product(Guests.Product, [Reported("Staging", Guests.Plus)], _configured);
        _unnamedEnvironment = DeclaredAudiencesOf.Product(Guests.Product, [Reported(EnvironmentName.NotSet, Guests.Preview)], _configured);
        _nothingKnown = DeclaredAudiencesOf.Product(Guests.Product, [], _notConfigured);
    }

    [Fact] void should_use_what_the_product_reported() => _reportedBeatsConfigured!.Select(_ => _.Value).ShouldEqual(["Plus"]);
    [Fact] void should_fall_back_to_the_configuration_when_nothing_was_reported() => _nothingReported!.Select(_ => _.Value).ShouldEqual(["Plus", "Preview"]);
    [Fact] void should_fall_back_to_the_configuration_for_another_environment() => _reportedFromAnotherEnvironment!.Select(_ => _.Value).ShouldEqual(["Plus", "Preview"]);
    [Fact] void should_take_the_only_report_of_a_product_that_names_no_environment() => _unnamedEnvironment!.Select(_ => _.Value).ShouldEqual(["Preview"]);
    [Fact] void should_know_nothing_when_nothing_is_reported_or_configured() => _nothingKnown.ShouldBeNull();
}
#endif
