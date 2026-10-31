// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;

namespace Cratis.Showrunner.BoxOffice.Compute.SetBudget;

/// <summary>
/// Sets how much of a meter the tenant may use in the allowance's period, below what the operator allows. Work is turned
/// away once the period's usage reaches the budget.
/// </summary>
/// <param name="Meter">The meter.</param>
/// <param name="Quantity">The most the tenant may use in the period.</param>
[Command]
[Authorize(Policy = BoxOfficePolicies.ManageCompute)]
public record SetComputeBudget(MeterId Meter, MeterQuantity Quantity) : ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => TenantStream.Id;

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="limits">What the maintainers limited, read fresh from the event log.</param>
    /// <returns><see cref="ComputeBudgetSet"/>, or nothing when the budget already is what was asked for.</returns>
    public IEnumerable<object> Handle(ComputeLimits? limits) =>
        limits?.For(Meter).Budget == Quantity ? [] : [new ComputeBudgetSet(Meter, Quantity)];
}

/// <summary>
/// Validates <see cref="SetComputeBudget"/>.
/// </summary>
public class SetComputeBudgetValidator : CommandValidator<SetComputeBudget>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetComputeBudgetValidator"/> class.
    /// </summary>
    /// <param name="allowances">What the operator allows the tenant.</param>
    /// <param name="offering">The product's offering.</param>
    public SetComputeBudgetValidator(Allowances? allowances, ProductOffering offering)
    {
        RuleFor(c => c.Meter)
            .Must(meter => offering.Meters.Any(_ => _.Id == meter))
            .WithMessage("The product does not declare this meter.");
        RuleFor(c => c.Quantity)
            .Must(quantity => quantity > 0)
            .WithMessage("A budget must be more than zero.");
        RuleFor(c => c)
            .Must(command => command.Quantity.Value <= (allowances?.For(command.Meter).Quantity.Value ?? 0))
            .WithMessage("A budget can not be more than the operator allows for the period.");
    }
}
