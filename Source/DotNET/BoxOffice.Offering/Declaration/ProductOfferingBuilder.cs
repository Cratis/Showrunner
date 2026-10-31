// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration;

/// <summary>
/// Declares what a product sells, in code.
/// </summary>
/// <example>
/// <code>
/// builder.Services.AddBoxOfficeOffering(offering => offering
///     .Product(Guid.Parse("6f9b2d3e-4f7a-4c52-9a4f-3c3f1d2a9e10"))
///     .Module("core", "Core", module => module.Availability(ModuleAvailability.Included).Owns("Acme.Core"))
///     .Module("analytics", "Analytics", module => module.Requires("core").Owns("Acme.Analytics").TrialDays(14))
///     .Meter("compute", "Compute", "minutes")
///     .Seats(SeatLicensing.PerUser));
/// </code>
/// </example>
public class ProductOfferingBuilder
{
    readonly List<ModuleBuilder> _modules = [];
    readonly List<MeterDefinition> _meters = [];
    ProductId? _product;
    SeatPolicy _seats = SeatPolicy.NotLicensed;

    /// <summary>
    /// Sets the product's identity, the event source its declaration is reported under.
    /// </summary>
    /// <param name="product">The product's id.</param>
    /// <returns>The builder for continuation.</returns>
    public ProductOfferingBuilder Product(ProductId product)
    {
        _product = product;
        return this;
    }

    /// <summary>
    /// Declares a module.
    /// </summary>
    /// <param name="id">The module's key.</param>
    /// <param name="name">The module's name as a person reads it.</param>
    /// <param name="configure">Describes what the module requires, owns and costs; a paid module without a trial by default.</param>
    /// <returns>The builder for continuation.</returns>
    public ProductOfferingBuilder Module(ModuleId id, ModuleName name, Action<ModuleBuilder>? configure = null)
    {
        var module = new ModuleBuilder(id, name);
        configure?.Invoke(module);
        _modules.Add(module);
        return this;
    }

    /// <summary>
    /// Declares a meter.
    /// </summary>
    /// <param name="id">The meter's key.</param>
    /// <param name="name">The meter's name as a person reads it.</param>
    /// <param name="unit">The unit it counts in.</param>
    /// <param name="hasConcurrencyLimit">Whether the product also limits how much a tenant uses at the same time.</param>
    /// <returns>The builder for continuation.</returns>
    public ProductOfferingBuilder Meter(MeterId id, MeterName name, MeterUnit unit, bool hasConcurrencyLimit = false)
    {
        _meters.Add(new(id, name, unit, hasConcurrencyLimit));
        return this;
    }

    /// <summary>
    /// Declares that the product licenses seats.
    /// </summary>
    /// <param name="licensing">Whether people need a seat.</param>
    /// <param name="roles">The tenant roles that need one; none means every role.</param>
    /// <returns>The builder for continuation.</returns>
    public ProductOfferingBuilder Seats(SeatLicensing licensing, params TenantRole[] roles)
    {
        _seats = new(licensing, roles);
        return this;
    }

    /// <summary>
    /// Builds the offering, checking it first.
    /// </summary>
    /// <returns>The <see cref="ProductOffering"/>.</returns>
    /// <exception cref="InvalidOffering">Thrown when the declaration cannot be sold, for example a module requires one the product does not have.</exception>
    public ProductOffering Build()
    {
        var offering = new ProductOffering(
            _product ?? ProductId.New(),
            [.. _modules.Select(_ => _.Build())],
            [.. _meters],
            _seats);

        var problems = OfferingProblems.In(offering, _product is not null).ToArray();
        if (problems.Length > 0)
        {
            throw new InvalidOffering(problems);
        }

        return offering;
    }
}
