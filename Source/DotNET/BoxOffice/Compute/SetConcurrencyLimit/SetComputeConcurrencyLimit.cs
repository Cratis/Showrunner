// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Offering.Declaration;

namespace Cratis.Showrunner.BoxOffice.Compute.SetConcurrencyLimit;

/// <summary>
/// Sets how many units of work of a meter the tenant may run at the same time. Only a meter the product declared with a
/// concurrency limit has one; work of the meter is turned away while the tenant runs that many.
/// </summary>
/// <param name="Meter">The meter.</param>
/// <param name="Limit">The number of units of work.</param>
[Command]
[Authorize(Policy = BoxOfficePolicies.ManageCompute)]
public record SetComputeConcurrencyLimit(MeterId Meter, int Limit) : ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => TenantStream.Id;

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="limits">What the maintainers limited, read fresh from the event log.</param>
    /// <returns><see cref="ComputeConcurrencyLimitSet"/>, or nothing when the limit already is what was asked for.</returns>
    public IEnumerable<object> Handle(ComputeLimits? limits) =>
        limits?.For(Meter).ConcurrencyLimit == Limit ? [] : [new ComputeConcurrencyLimitSet(Meter, Limit)];
}

/// <summary>
/// Validates <see cref="SetComputeConcurrencyLimit"/>.
/// </summary>
public class SetComputeConcurrencyLimitValidator : CommandValidator<SetComputeConcurrencyLimit>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetComputeConcurrencyLimitValidator"/> class.
    /// </summary>
    /// <param name="offering">The product's offering.</param>
    public SetComputeConcurrencyLimitValidator(ProductOffering offering)
    {
        RuleFor(c => c.Meter)
            .Must(meter => offering.Meters.Any(_ => _.Id == meter && _.HasConcurrencyLimit))
            .WithMessage("The product does not declare this meter with a concurrency limit.");
        RuleFor(c => c.Limit)
            .GreaterThan(0)
            .WithMessage("The limit must be at least one.");
    }
}
